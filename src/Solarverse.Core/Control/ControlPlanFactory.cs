using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System.Drawing;
using System.Reflection;

namespace Solarverse.Core.Control
{
    public class ControlPlanFactory : IControlPlanFactory
    {
        private readonly ICurrentDataService _currentDataService;
        private readonly ILogger<ControlPlanFactory> _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ICurrentTimeProvider _currentTimeProvider;

        public ControlPlanFactory(ICurrentDataService currentDataService, ILogger<ControlPlanFactory> logger, IConfigurationProvider configurationProvider, ICurrentTimeProvider currentTimeProvider)
        {
            _currentDataService = currentDataService;
            _logger = logger;
            _configurationProvider = configurationProvider;
            _currentTimeProvider = currentTimeProvider;
        }

        public void CheckForAdaptations(InverterCurrentState currentState)
        {
            if (!_currentDataService.TimeSeries.Any(x => x.IsDischargeTarget))
            {
                _logger.LogInformation("Will not check for adaptations before discharge targets are set.");
                return;
            }

            // happens after every current state update - here we check if the battery charge is on track and if we need another 1/2 hour charge period
            if (_currentDataService.TimeSeries.Where(x => x.IncomingRate.HasValue && x.IsFuture(_currentTimeProvider)).Any())
            {
                using var updateLock = _currentDataService.LockForUpdate();

                _currentDataService.TimeSeries.Where(x => x.IsFuture(_currentTimeProvider)).Each(x => x.ControlAction = null);
                _currentDataService.TimeSeries.Where(x => x.IsFuture(_currentTimeProvider) && x.IsDischargeTarget && x.RequiredPowerKwh > 0).Each(x => x.ControlAction = ControlAction.Discharge);

                CreatePlanForDischargeTargets();
            }
        }

        public void CreatePlan()
        {
            SetDischargeTargets();

            using var updateLock = _currentDataService.LockForUpdate();
            CreatePlanForDischargeTargets();
        }

        public void SetDischargeTargets()
        {
            // happns after tariffs become available, so we look at solar forecast, predicted consumption and tariff rates
            var firstTime = _currentTimeProvider.CurrentPeriodStartUtc;

            if (!_currentDataService.TimeSeries
                                    .Where(x => x.Time >= firstTime)
                                    .Where(x => !x.ControlAction.HasValue && x.IncomingRate.HasValue)
                                    .Any())
            {
                _logger.LogInformation($"Creating control plan firstTime = {firstTime} - no points to update");
                return;
            }

            SetDischargeTargets(firstTime);
        }

        private void SetDischargeTargets(DateTime firstTime)
        {
            _currentDataService.TimeSeries.Set(x => x.RequiredBatteryPowerKwh = null);

            _logger.LogInformation($"Creating control plan firstTime = {firstTime}");

            var tariffRates = _currentDataService.TimeSeries
                .GetSeries(x => x.IncomingRate)
                .Where(x => x.Time >= firstTime)
                .ToList();

            var count = tariffRates.Count;
            var ratesByGroup = tariffRates.GroupBy(x => x.Value).OrderByDescending(x => x.Key).ToList();
            _logger.LogInformation($"{count} tariff rate periods in {ratesByGroup.Count} groups");

            var dischargeTimesSet = 0;
            foreach (var rate in ratesByGroup)
            {
                _logger.LogInformation($"Targeting rate group with rate {rate.Key:N2} and {rate.Count()} entries");
                foreach (var point in rate)
                {
                    _currentDataService.TimeSeries.Set(point.Time, x =>
                    {
                        _logger.LogInformation($"Targeting discharge for {point.Time}");
                        x.ControlAction = ControlAction.Discharge;
                        x.IsDischargeTarget = true;
                    });
                    dischargeTimesSet++;
                }

                if (dischargeTimesSet >= count / 2)
                {
                    break;
                }
            }
        }

        private void CreatePlanForDischargeTargets()
        {
            // now we want to set the 'required battery power' for the periods that we want to discharge
            var forecastPoints = _currentDataService.GetForecastTimeSeries(_logger);
            if (!forecastPoints.Any())
            {
                return;
            }

            _currentDataService.RecalculateForecast();

            SetRequiredPower(forecastPoints);

            // now we make a first pass on the discharge points - find any shortfalls
            // and try to fill them in from points that don't have a current control action
            RunPass("First pass", forecastPoints, x => !x.ControlAction.HasValue);

            // now we make a second pass - the point of this pass is to turn any discharge
            // slots to charge if we have a shortfall somewhere
            RunPass("Second pass", forecastPoints, x => x.ControlAction != ControlAction.Charge);

            // now make sure that the covered discharge periods are catered for - if more expensive
            // periods aren't fully covered, we sacrifice charge for less expensive periods
            PrioritizeDischargeTargetsByPrice(forecastPoints);

            // set the remaining points to hold/discharge
            SetTailPoints(forecastPoints);

            // now run pairing - identify any periods left where we can pair with another
            // period to charge / discharge cost-effectively
            RunPairingPass(forecastPoints);

            _currentDataService.RecalculateForecast();
        }

        private void RunPairingPass(ForecastTimeSeries series)
        {
            // For each remaining point, order them by cost. If there is a chance for us to discharge at
            // higher cost and recharge at lower cost, we should do that. This should take into account
            // whether the pair of points being targeted are split by any sections where the battery is
            // already at 100%
            var remainingPoints = series
                .Where(x => x.ControlAction == ControlAction.Hold && 
                            x.IncomingRate.HasValue &&
                            x.IsFuture(_currentTimeProvider))
                .OrderBy(x => x.IncomingRate)
                .ToList();

            foreach (var point in remainingPoints)
            {
                if (point.ControlAction != ControlAction.Hold)
                {
                    continue;
                }

                var passPoints = remainingPoints.Where(x => x.ControlAction == ControlAction.Hold).ToList();
                var availableChargePercent = (100 - point.ForecastBatteryPercentage ?? series.Reserve);
                var periodCharge = Math.Min(availableChargePercent, series.MaxChargeKwhPerPeriod);
                if (periodCharge > 0)
                {
                    if (!RunSinglePointPass(point, passPoints, series.Efficiency, series.Capacity, periodCharge, series.Reserve))
                    {
                        return;
                    }
                }
            }
        }

        private static void SetTailPoints(ForecastTimeSeries series)
        {
            // now set all remaining points to discharge if there is excess power,
            // or hold if there is not
            var pointWithControlActionPassed = false;
            foreach (var point in series)
            {
                if (point.ActualConsumptionKwh.HasValue)
                {
                    continue;
                }

                if (!point.IncomingRate.HasValue)
                {
                    continue;
                }

                if (!point.ControlAction.HasValue)
                {
                    if (pointWithControlActionPassed)
                    {
                        point.ControlAction = point.ExcessPowerKwh.HasValue && point.ExcessPowerKwh.Value > 0 ?
                            ControlAction.Discharge : ControlAction.Hold;
                    }
                    else
                    {
                        // at the end of the plan, we just set discharge
                        point.ControlAction = ControlAction.Discharge;
                    }
                }
                else
                {
                    pointWithControlActionPassed = true;
                }    
            }
        }

        private void SetRequiredPower(IEnumerable<TimeSeriesPoint> forecastPoints)
        {
            var lastPointPower = 0d;
            foreach (var point in forecastPoints)
            {
                if (point.ControlAction == ControlAction.Discharge)
                {
                    var pointPower = lastPointPower + (point.RequiredPowerKwh ?? 0);
                    point.RequiredBatteryPowerKwh = lastPointPower = pointPower;
                    _logger.LogInformation($"Point at {point.Time} has ideal required power of {point.RequiredBatteryPowerKwh:N2} kWh");
                }
                else
                {
                    lastPointPower = 0;
                }

                // todo - this should look at if we have solar excess later, and if it's profitable to export
                if (point.IncomingRate.HasValue && point.IncomingRate.Value < 0.0)
                {
                    _logger.LogInformation($"Point at {point.Time} has negative rate, setting to charge");
                    point.ControlAction = ControlAction.Charge;
                }
            }

            _currentDataService.RecalculateForecast();
        }

        private bool RunSinglePointPass(TimeSeriesPoint potentialChargePoint, List<TimeSeriesPoint> points, double efficiency, double capacity, double kwhPerPeriod, int reserve)
        {
            var chargeSlotPercent = (kwhPerPeriod / capacity) * 100;

            var eligibleDischargePointsPrior = points
                .Where(x => x.Time < potentialChargePoint.Time)
                .TakeWhile(x => x.ForecastBatteryPercentage > chargeSlotPercent + reserve);

            var eligibleDischargePointsAfter = points
                .Where(x => x.Time > potentialChargePoint.Time)
                .OrderBy(x => x.Time)
                .TakeWhile(x => x.ForecastBatteryPercentage < 100 - chargeSlotPercent);

            var eligibleDischargePoints = eligibleDischargePointsAfter
                .Concat(eligibleDischargePointsPrior)
                .Where(x => !x.IsDischargeTarget)
                .Where(x => x.RequiredPowerKwh > 0)
                .OrderByDescending(x => x.IncomingRate)
                .ToList();

            var dischargePoints = new List<TimeSeriesPoint>();
            var currentTotalKwh = 0.0;
            var totalCost = 0.0;
            var targetMet = false;
            foreach (var point in eligibleDischargePoints)
            {
                var pointPower = point.RequiredPowerKwh ?? 0;

                if (currentTotalKwh + pointPower > kwhPerPeriod * 1.1)
                {
                    targetMet = true;

                    if (dischargePoints.Count == 0)
                    {
                        dischargePoints.Add(point);
                        currentTotalKwh += pointPower;
                        totalCost += pointPower * point.IncomingRate ?? 0;
                    }
                    break;
                }

                dischargePoints.Add(point);
                currentTotalKwh += pointPower;
                totalCost += pointPower * point.IncomingRate ?? 0;
            }

            if (!dischargePoints.Any())
            {
                _logger.LogInformation($"(Pairing) No targetable periods found");
                return false;
            }

            var targetString = string.Join(',', dischargePoints.Select(x => x.Time.ToString("MM/dd HH:mm")));

            if (!targetMet || currentTotalKwh < kwhPerPeriod * 0.8)
            {
                _logger.LogInformation($"(Pairing) Periods at [{targetString}] did not meet point targeting ({currentTotalKwh:N1} < {kwhPerPeriod:N1} * 0.8)");
                return false;
            }

            var totalCostPerKwh = totalCost / currentTotalKwh;
            _logger.LogInformation($"(Pairing) Targeting periods at [{targetString}] with a total cost of {totalCost:N2}, cost per kwh of {totalCostPerKwh:N2} and required power of {currentTotalKwh:N1}");

            var selectedPointCost = potentialChargePoint.IncomingRate;
            if (selectedPointCost / efficiency > totalCostPerKwh)
            {
                _logger.LogInformation($"(Pairing) Period at {potentialChargePoint.Time} is too expensive ({(selectedPointCost / efficiency):N2})");
                return false;
            }

            _logger.LogInformation($"(Pairing) Setting period {potentialChargePoint.Time} to charge to cater for targeted points");
            dischargePoints.Each(x => x.ControlAction = ControlAction.Discharge);
            potentialChargePoint.ControlAction = ControlAction.Charge;
            _currentDataService.RecalculateForecast();

            return true;
        }

        private void PrioritizeDischargeTargetsByPrice(ForecastTimeSeries points)
        {
            points.RunActionOnDischargeStartPeriods("Prioritize", period =>
            {
                var dischargePoint = period.Point;
                var dischargePointPercent = period.PointPercentRequired;
                var averageCost = period.DischargePoints.Average(x => x.IncomingRate);

                while (true)
                {
                    var shortfallPercent = dischargePointPercent - (dischargePoint.ForecastBatteryPercentage ?? 0);
                    var shortfallKwh = points.Capacity * shortfallPercent / 100;

                    if (shortfallKwh <= 0)
                    {
                        break;
                    }

                    _logger.LogInformation($"(Prioritize) Point at {dischargePoint.Time} has forecast of {dischargePoint.ForecastBatteryPercentage:N1}% battery, charge required - shortfall of {shortfallPercent:N1}%, {shortfallKwh:N2} kWh");

                    var eligibleHoldPoints = points
                        .Where(x => x.Time < dischargePoint.Time)
                        .TakeWhile(x => x.ForecastBatteryPercentage < 100)
                        .Where(x => x.ControlAction != ControlAction.Hold && x.ControlAction != ControlAction.Charge)
                        .Where(x => x.IncomingRate < averageCost)
                        .OrderBy(x => x.IncomingRate)
                        .Take(1)
                        .ToList();

                    if (!eligibleHoldPoints.Any())
                    {
                        _logger.LogInformation($"(Prioritize) No eligible points found");
                        break;
                    }

                    // check if the point we're transforming has a rate low enough to make 
                    // it worth charging
                    var selectedPoint = eligibleHoldPoints[0];
                    _logger.LogInformation($"(Prioritize) Setting period {selectedPoint.Time} to hold");
                    selectedPoint.ControlAction = ControlAction.Hold;

                    _currentDataService.RecalculateForecast();
                }
            });
        }

        private void RunPass(string passName, ForecastTimeSeries series, Func<TimeSeriesPoint, bool> selector)
        {
            series.RunActionOnDischargeStartPeriods(passName, period =>
            {
                var dischargePoint = period.Point;
                var dischargePointPercent = period.PointPercentRequired;

                if (series.MaxChargeKwhPerPeriod <= 0)
                {
                    return;
                }

                // bucketize the discharge points, so we can work out if each charge period is cost effective
                var buckets = Bucketizer.Bucketize(period.DischargePoints, series.MaxChargeKwhPerPeriod);

                while (true)
                {
                    var shortfallPercent = dischargePointPercent - (dischargePoint.ForecastBatteryPercentage ?? 0);
                    var shortfallKwh = series.Capacity * shortfallPercent / 100;

                    if (shortfallKwh <= 0)
                    {
                        break;
                    }

                    _logger.LogInformation($"({passName}) Point at {dischargePoint.Time} has forecast of {dischargePoint.ForecastBatteryPercentage:N1}% battery, charge required - shortfall of {shortfallPercent:N1}%, {shortfallKwh:N2} kWh");

                    var eligibleChargePoints = series
                        .Where(x => x.Time < dischargePoint.Time)
                        .TakeWhile(x => x.ForecastBatteryPercentage < 100)
                        .Where(selector)
                        .Where(x => !x.IsDischargeTarget)
                        .OrderBy(x => x.IncomingRate)
                        .Take(1)
                        .ToList();

                    if (!eligibleChargePoints.Any() || !buckets.Any())
                    {
                        _logger.LogInformation($"({passName}) No eligible points found");
                        break;
                    }

                    // check if the point we're transforming has a rate low enough to make 
                    // it worth charging
                    var selectedPoint = eligibleChargePoints[0];
                    if (buckets.TakeSlot(selectedPoint.IncomingRate, series.Efficiency))
                    {
                        _logger.LogInformation($"({passName}) Setting period {selectedPoint.Time} to charge");
                        selectedPoint.ControlAction = ControlAction.Charge;

                        var chargeLeft = series.MaxChargeKwhPerPeriod - shortfallKwh;
                        while (chargeLeft > 0)
                        {
                            _logger.LogInformation($"({passName}) {chargeLeft:N2} kWh left to fill");

                            var potentialDischargePoints =
                                series.Where(x => x.IsFuture(_currentTimeProvider) && (!x.ControlAction.HasValue || x.ControlAction == ControlAction.Hold) && x.RequiredPowerKwh < chargeLeft && x.RequiredPowerKwh > 0 && x.IncomingRate.HasValue).ToList();

                            var eligibleDischargePointsPrior = potentialDischargePoints
                                .Where(x => x.Time < selectedPoint.Time)
                                .TakeWhile(x => x.ForecastBatteryPercentage > ((x.RequiredPowerKwh / series.Capacity) * 100) + series.Reserve);

                            var eligibleDischargePointsAfter = potentialDischargePoints
                                .Where(x => x.Time > selectedPoint.Time)
                                .OrderBy(x => x.Time)
                                .TakeWhile(x => x.ForecastBatteryPercentage < 100);

                            var potentialDischargePoint = eligibleDischargePointsPrior
                                .Concat(eligibleDischargePointsAfter)
                                .OrderByDescending(x => x.IncomingRate)
                                .FirstOrDefault();

                            if (potentialDischargePoint == null)
                            {
                                _logger.LogInformation($"({passName}) No eligible points left for filling");
                                break;
                            }

                            _logger.LogInformation($"({passName}) Setting period {potentialDischargePoint.Time} to discharge for filling");
                            potentialDischargePoint.ControlAction = ControlAction.Discharge;
                            chargeLeft -= potentialDischargePoint.RequiredPowerKwh ?? 0;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"({passName}) Period at {selectedPoint.Time} is too expensive to charge");

                        // if we have already passed the point of it being worthwhile charging
                        // then there's no point continuing;
                        break;
                    }

                    _currentDataService.RecalculateForecast();
                }
            });
        }
    }
}
