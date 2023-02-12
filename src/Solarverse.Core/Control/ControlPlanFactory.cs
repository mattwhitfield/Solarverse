using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;

namespace Solarverse.Core.Control
{
    public class ControlPlanFactory : IControlPlanFactory
    {
        private readonly ICurrentDataService _currentDataService;
        private readonly ILogger<ControlPlanFactory> _logger;
        private readonly IConfigurationProvider _configurationProvider;

        public ControlPlanFactory(ICurrentDataService currentDataService, ILogger<ControlPlanFactory> logger, IConfigurationProvider configurationProvider)
        {
            _currentDataService = currentDataService;
            _logger = logger;
            _configurationProvider = configurationProvider;
        }

        public void CheckForAdaptations(InverterCurrentState currentState)
        {
            if (!_currentDataService.TimeSeries.Any(x => x.IsDischargeTarget))
            {
                _logger.LogInformation("Will not check for adaptations before discharge targets are set.");
                return;
            }

            // happens after every current state update - here we check if the battery charge is on track and if we need another 1/2 hour charge period
            if (_currentDataService.TimeSeries.Where(x => !x.ActualConsumptionKwh.HasValue).Any())
            {
                using var updateLock = _currentDataService.LockForUpdate();

                var currentTime = new Period(TimeSpan.FromMinutes(30)).GetLast(DateTime.UtcNow);

                _currentDataService.TimeSeries.Where(x => !x.ActualConsumptionKwh.HasValue && x.Time > currentTime).Each(x => x.ControlAction = null);
                _currentDataService.TimeSeries.Where(x => !x.ActualConsumptionKwh.HasValue && x.Time > currentTime && x.IsDischargeTarget && x.RequiredPowerKwh > 0).Each(x => x.ControlAction = ControlAction.Discharge);

                CreatePlanForDischargeTargets();
            }
        }

        public void CreatePlan()
        {
            // happns after tariffs become available, so we look at solar forecast, predicted consumption and tariff rates
            var firstTime = _currentDataService.TimeSeries.GetMinimumDate(x => !x.ActualConsumptionKwh.HasValue);
            if (firstTime == null)
            {
                firstTime = new Period(TimeSpan.FromMinutes(30)).GetNext(DateTime.UtcNow);
            }

            if (!_currentDataService.TimeSeries
                                    .Where(x => x.Time >= firstTime)
                                    .Where(x => !x.ControlAction.HasValue && x.IncomingRate.HasValue)
                                    .Any())
            {
                _logger.LogInformation($"Creating control plan firstTime = {firstTime} - no points to update");
                return;
            }

            using var updateLock = _currentDataService.LockForUpdate();

            SetDischargeTargets(firstTime.Value);

            CreatePlanForDischargeTargets();
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
                    if (_currentDataService.TimeSeries.TryGetDataPointFor(point.Time, out var existing))
                    {
                        // don't set discharge when we have excess power
                        if (existing.ExcessPowerKwh > 0)
                        {
                            continue;
                        }
                    }
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
            var allPointsWithoutActualFiguresReversed = _currentDataService.TimeSeries
                .OrderByDescending(x => x.Time)
                .Where(x => !x.ActualConsumptionKwh.HasValue)
                .ToList();
            if (!allPointsWithoutActualFiguresReversed.Any())
            {
                return;
            }

            _currentDataService.RecalculateForecast();
            SetRequiredPower(allPointsWithoutActualFiguresReversed);

            var efficiency = _configurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            var maxChargeKwhPerPeriod = _currentDataService.CurrentState.MaxChargeRateKw * 0.5 * efficiency;
            var capacity = _configurationProvider.Configuration.Battery.CapacityKwh ?? 5;

            // now we make a first pass on the discharge points - find any shortfalls and try to fill them in from points that don't have a current control action
            RunPass("First pass", allPointsWithoutActualFiguresReversed, x => !x.ControlAction.HasValue, efficiency, capacity, maxChargeKwhPerPeriod);

            // now we make a second pass - the point of this pass is to turn any discharge slots to charge if we have a shortfall somewhere
            RunPass("Second pass", allPointsWithoutActualFiguresReversed, x => x.ControlAction != ControlAction.Charge, efficiency, capacity, maxChargeKwhPerPeriod);

            RunPairingPass(allPointsWithoutActualFiguresReversed, efficiency, capacity);

            PrioritizeDischargeTargetsByPrice(allPointsWithoutActualFiguresReversed, efficiency, capacity, maxChargeKwhPerPeriod);

            SetTailPoints(allPointsWithoutActualFiguresReversed);

            _currentDataService.RecalculateForecast();
        }

        private void RunPairingPass(List<TimeSeriesPoint> allPointsWithoutActualFiguresReversed, double efficiency, double capacity)
        {
            // For each remaining point, order them by cost. If there is a chance for us to discharge at
            // higher cost and recharge at lower cost, we should do that. This should take into account
            // whether the pair of points being targeted are split by any sections where the battery is
            // already at 100%
            var remainingPoints = allPointsWithoutActualFiguresReversed
                .Where(x => !x.ControlAction.HasValue)
                .OrderByDescending(x => x.IncomingRate)
                .ToList();

            foreach (var point in remainingPoints)
            {
                if (!RunSinglePointPass(point, remainingPoints, efficiency, capacity))
                {
                    break;
                }
            }
        }

        private static void SetTailPoints(List<TimeSeriesPoint> allPointsWithoutActualFigures)
        {
            // now set all remaining points to discharge if there is excess power,
            // or hold if there is not
            var pointWithControlActionPassed = false;
            foreach (var point in allPointsWithoutActualFigures)
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

        private void SetRequiredPower(List<TimeSeriesPoint> allPointsWithoutActualFiguresReversed)
        {
            var lastPointPower = 0d;
            foreach (var point in allPointsWithoutActualFiguresReversed)
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

                if (point.IncomingRate.HasValue && point.IncomingRate.Value < 0)
                {
                    _logger.LogInformation($"Point at {point.Time} has negative rate, setting to charge");
                    point.ControlAction = ControlAction.Charge;
                }
            }
        }

        private bool RunSinglePointPass(TimeSeriesPoint targetPoint, List<TimeSeriesPoint> points, double efficiency, double capacity)
        {
            if (!targetPoint.RequiredPowerKwh.HasValue || targetPoint.RequiredPowerKwh.Value <= 0)
            {
                return false;
            }

            var targetPointPercent = targetPoint.RequiredPowerKwh.Value / capacity;

            var eligibleChargePoints = points
                .Where(x => x.Time <= targetPoint.Time)
                .TakeWhile(x => x.ForecastBatteryPercentage < 100 - targetPointPercent)
                .Where(x => !x.IsDischargeTarget)
                .OrderBy(x => x.IncomingRate)
                .Take(1)
                .ToList();

            if (!eligibleChargePoints.Any())
            {
                return false;
            }

            // check if the point we're transforming has a rate low enough to make 
            // it worth charging
            var selectedPoint = eligibleChargePoints[0];
            if (selectedPoint.IncomingRate >= targetPoint.IncomingRate * efficiency)
            {
                _logger.LogInformation($"(Pairing) Period at {selectedPoint.Time} is too expensive to charge");
                return false;
            }

            _logger.LogInformation($"(Pairing) Setting period {selectedPoint.Time} to charge to cater for point at {targetPoint.Time}");
            selectedPoint.ControlAction = ControlAction.Charge;
            _currentDataService.RecalculateForecast();

            return true;
        }

        private void PrioritizeDischargeTargetsByPrice(List<TimeSeriesPoint> points, double efficiency, double capacity, double maxChargeKwhPerPeriod)
        {
            var lastPoint = points.First();
            foreach (var point in points.Skip(1))
            {
                if (lastPoint.ControlAction == ControlAction.Discharge &&
                    point.ControlAction != ControlAction.Discharge &&
                    lastPoint.RequiredBatteryPowerKwh.HasValue)
                {
                    var lastPointPercent =
                        (((lastPoint.RequiredBatteryPowerKwh.Value / efficiency) / capacity) * 100) +
                        _currentDataService.CurrentState.BatteryReserve;
                    if (lastPointPercent > 100)
                    {
                        lastPointPercent = 100;
                    }

                    if (lastPoint.ForecastBatteryPercentage < lastPointPercent)
                    {
                        _logger.LogInformation($"(Prioritize) Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh, {lastPointPercent:N1}%");

                        // we now work out how much each period costs, with each period being defined as the usage
                        // of power that we could charge in one single half hour period
                        var dischargePoints = points
                            .Where(x => x.Time > point.Time)
                            .OrderBy(x => x.Time)
                            .TakeWhile(x => x.ControlAction == ControlAction.Discharge && x.ExcessPowerKwh < 0);

                        var averageCost = dischargePoints.Average(x => x.IncomingRate);

                        while (true)
                        {
                            var shortfallPercent = lastPointPercent - (lastPoint.ForecastBatteryPercentage ?? 0);
                            var shortfallKwh = capacity * shortfallPercent / 100;

                            if (shortfallKwh <= 0)
                            {
                                break;
                            }

                            _logger.LogInformation($"(Prioritize) Point at {lastPoint.Time} has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery, charge required - shortfall of {shortfallPercent:N1}%, {shortfallKwh:N2} kWh");

                            var eligibleHoldPoints = points
                                .Where(x => x.Time <= point.Time)
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
                    }
                }

                lastPoint = point;
            }
        }

        private void RunPass(string passName, List<TimeSeriesPoint> points, Func<TimeSeriesPoint, bool> selector, double efficiency, double capacity, double maxChargeKwhPerPeriod)
        {
            var lastPoint = points.First();
            foreach (var point in points.Skip(1))
            {
                if (lastPoint.ControlAction == ControlAction.Discharge &&
                    point.ControlAction != ControlAction.Discharge &&
                    lastPoint.RequiredBatteryPowerKwh.HasValue)
                {
                    var lastPointPercent =
                        (((lastPoint.RequiredBatteryPowerKwh.Value / efficiency) / capacity) * 100) +
                        _currentDataService.CurrentState.BatteryReserve;
                    if (lastPointPercent > 100)
                    {
                        lastPointPercent = 100;
                    }

                    if (lastPoint.ForecastBatteryPercentage >= lastPointPercent)
                    {
                        _logger.LogInformation($"({passName}) Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh, {lastPointPercent:N1}% but has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery - no charge needed");
                    }
                    else
                    {
                        _logger.LogInformation($"({passName}) Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh, {lastPointPercent:N1}%");

                        // we now work out how much each period costs, with each period being defined as the usage
                        // of power that we could charge in one single half hour period
                        var dischargePoints = points
                            .Where(x => x.Time > point.Time)
                            .OrderBy(x => x.Time)
                            .TakeWhile(x => x.ControlAction == ControlAction.Discharge && x.ExcessPowerKwh < 0);

                        var buckets = Bucketizer.Bucketize(dischargePoints, maxChargeKwhPerPeriod);

                        while (true)
                        {
                            var shortfallPercent = lastPointPercent - (lastPoint.ForecastBatteryPercentage ?? 0);
                            var shortfallKwh = capacity * shortfallPercent / 100;

                            if (shortfallKwh <= 0)
                            {
                                break;
                            }

                            _logger.LogInformation($"({passName}) Point at {lastPoint.Time} has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery, charge required - shortfall of {shortfallPercent:N1}%, {shortfallKwh:N2} kWh");

                            var eligibleChargePoints = points
                                .Where(x => x.Time <= point.Time)
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
                            if (buckets.TakeSlot(selectedPoint.IncomingRate, efficiency))
                            {
                                _logger.LogInformation($"({passName}) Setting period {selectedPoint.Time} to charge");
                                selectedPoint.ControlAction = ControlAction.Charge;
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
                    }
                }

                lastPoint = point;
            }
        }
    }
}
