using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System.Runtime.Serialization;

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
            // happens after every current state update - here we check if the battery charge is on track and if we need another 1/2 hour charge period
            // TODO
        }

        public void CreatePlan()
        {
            // happns after tariffs become available, so we look at solar forecast, predicted consumption and tariff rates
            var minTime = _currentDataService.TimeSeries.GetMinimumDate(x => x.ActualConsumptionKwh.HasValue);
            if (minTime == null)
            {
                minTime = DateTime.UtcNow;
            }

            var firstTime = _currentDataService.TimeSeries.GetMinimumDate(x => !x.ActualConsumptionKwh.HasValue); 
            if (firstTime == null)
            {
                firstTime = new Period(TimeSpan.FromMinutes(30)).GetNext(DateTime.UtcNow);
            }

            var periodsWithoutControl = _currentDataService.TimeSeries
                .Where(x => x.Time >= firstTime)
                .Where(x => !x.ControlAction.HasValue && x.IncomingRate.HasValue)
                .Count();

            if (periodsWithoutControl == 0)
            {
                _logger.LogInformation($"Creating control plan minTime = {minTime}, firstTime = {firstTime} - no points to update");
                return;
            }

            using var updateLock = _currentDataService.LockForUpdate();
            _currentDataService.TimeSeries.Set(x => x.RequiredBatteryPowerKwh = null);

            _logger.LogInformation($"Creating control plan minTime = {minTime}, firstTime = {firstTime}");

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

            // now we want to set the 'required battery power' for the periods that we want to discharge
            var allPointsWithoutActualFiguresReversed = _currentDataService.TimeSeries
                .OrderByDescending(x => x.Time)
                .Where(x => !x.ActualConsumptionKwh.HasValue)
                .ToList();
            if (!allPointsWithoutActualFiguresReversed.Any())
            {
                return;
            }

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

            var efficiency = _configurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            var maxChargeKwhPerPeriod = _currentDataService.CurrentState.MaxChargeRateKw * 0.5 * efficiency;
            var allPointsWithoutActualFigures = allPointsWithoutActualFiguresReversed.AsEnumerable().Reverse().ToList();
            var capacity = _configurationProvider.Configuration.Battery.CapacityKwh ?? 5;
            var singleDirectionEfficieny = 1 - ((1 - efficiency) / 2);

            _currentDataService.RecalculateForecast();

            // now go through backwards, where we have transitions from discharge to no plan
            // evaluate the difference between forecast battery percentage and the required 
            // battery percentage

            var adjustedCapacity = capacity * efficiency;
            var carryForward = adjustedCapacity;
            var lastPoint = allPointsWithoutActualFigures.First();
            var lastCarryForward = 0d;
            foreach (var point in allPointsWithoutActualFigures.Skip(1))
            {
                if (point.RequiredBatteryPowerKwh.HasValue && !lastPoint.RequiredBatteryPowerKwh.HasValue)
                {
                    var spare = adjustedCapacity - point.RequiredBatteryPowerKwh.Value;
                    if (spare < 0)
                    {
                        spare = 0;
                    }
                    carryForward = spare;
                }

                point.MaxCarryForwardChargeKwh = carryForward;
                lastPoint = point;

                if (carryForward != lastCarryForward)
                {
                    _logger.LogInformation($"From {point.Time:HH:mm} maximum carry forward charge is {point.MaxCarryForwardChargeKwh:N2} kWh");
                    lastCarryForward = carryForward;
                }
            }

            lastPoint = allPointsWithoutActualFiguresReversed.First();
            foreach (var point in allPointsWithoutActualFiguresReversed.Skip(1))
            {
                if (lastPoint.ControlAction == ControlAction.Discharge &&
                    point.ControlAction != ControlAction.Discharge &&
                    lastPoint.RequiredBatteryPowerKwh.HasValue)
                {
                    _logger.LogInformation($"Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh");

                    var lastPointPercent = 
                        (((lastPoint.RequiredBatteryPowerKwh.Value / efficiency) / capacity) * 100) +
                        _currentDataService.CurrentState.BatteryReserve;
                    if (lastPointPercent > 100)
                    {
                        lastPointPercent = 100;
                    }
                    _logger.LogInformation($"Point at {lastPoint.Time} requires {lastPointPercent:N1}% battery");

                    if (lastPoint.ForecastBatteryPercentage >= lastPointPercent)
                    {
                        _logger.LogInformation($"Point at {lastPoint.Time} has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery, no charge needed");
                    }
                    else
                    {
                        var shortfallPercent = lastPointPercent - (lastPoint.ForecastBatteryPercentage ?? 0);
                        var shortfallKwh = capacity * shortfallPercent / 100;

                        var numberOfChargePeriodsRequired = (int)Math.Ceiling(shortfallKwh / maxChargeKwhPerPeriod);
                        _logger.LogInformation($"Point at {lastPoint.Time} has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery, charge required - shortfall of {shortfallPercent:N1}%, {shortfallKwh:N2} kWh, in {numberOfChargePeriodsRequired} periods");

                        var dischargePoints = allPointsWithoutActualFiguresReversed
                            .Where(x => x.Time > point.Time)
                            .OrderBy(x => x.Time)
                            .TakeWhile(x => x.ControlAction == ControlAction.Discharge && x.ExcessPowerKwh < 0);

                        var buckets = Bucketizer.Bucketize(dischargePoints, maxChargeKwhPerPeriod);

                        // if we need more charge then we can carry forward, then we
                        // need to add some charge afterwards. So the eligible points in 
                        // this instance are the ones where the max carry forward hasn't changed
                        if (point.MaxCarryForwardChargeKwh.HasValue && shortfallKwh > point.MaxCarryForwardChargeKwh)
                        {
                            _logger.LogInformation($"We need more charge than we can carry forward - {shortfallKwh:N2} > {point.MaxCarryForwardChargeKwh:N2}");

                            var allPointsAfterMax = allPointsWithoutActualFiguresReversed
                                .Where(x => x.Time <= point.Time)
                                .TakeWhile(x => x.MaxCarryForwardChargeKwh == point.MaxCarryForwardChargeKwh)
                                .ToList();

                            _logger.LogInformation($"Considering points from  {allPointsAfterMax.Select(x => x.Time).Min()} to {allPointsAfterMax.Select(x => x.Time).Max()}");

                            var amountToChargeNow = shortfallKwh - point.MaxCarryForwardChargeKwh.Value;
                            var numberOfPeriodsAfterMax = (int)Math.Ceiling(amountToChargeNow / maxChargeKwhPerPeriod);
                            _logger.LogInformation($"Amount to charge now in range is {amountToChargeNow:N2} kWh, in {numberOfPeriodsAfterMax} periods");

                            var pointsAfterMax = allPointsAfterMax
                                .Where(x => !x.ControlAction.HasValue)
                                .ToList();

                            // now find the points in there with the lowest incoming rate
                            foreach (var targetPoint in pointsAfterMax.Where(x => x.IncomingRate.HasValue).OrderBy(x => x.IncomingRate))
                            {
                                if (numberOfPeriodsAfterMax > 0)
                                {
                                    if (buckets.TakeSlot(targetPoint.IncomingRate, efficiency))
                                    {
                                        _logger.LogInformation($"Setting period {targetPoint.Time} to charge");

                                        targetPoint.ControlAction = ControlAction.Charge;
                                        numberOfChargePeriodsRequired--;
                                        numberOfPeriodsAfterMax--;
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"Period {targetPoint.Time} is too expensive to charge from");
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            var chargePoints = allPointsAfterMax
                                .Where(x => x.ControlAction == ControlAction.Charge)
                                .Select(x => x.Time).ToList();

                            if (chargePoints.Any())
                            {
                                // now account for any points after we have charged within the current set
                                // where it is set to discharge, and add extra charge
                                var minTimeWithChargeAction = chargePoints.Min();

                                // find any points that are discharging after our charge has started
                                var kWhToAccountForDischarge = allPointsAfterMax
                                    .Where(x => x.Time > minTimeWithChargeAction)
                                    .Where(x => x.ControlAction == ControlAction.Discharge)
                                    .Select(x => x.RequiredPowerKwh ?? 0)
                                    .Sum();

                                var extraChargePeriodsRequired = (int)Math.Ceiling(kWhToAccountForDischarge / maxChargeKwhPerPeriod);
                                _logger.LogInformation($"Accounting for {kWhToAccountForDischarge:N2} kWh of discharge within the period, {extraChargePeriodsRequired} extra charge periods required");

                                foreach (var targetPoint in pointsAfterMax.Where(x => x.IncomingRate.HasValue).OrderBy(x => x.IncomingRate))
                                {
                                    if (targetPoint.ControlAction.HasValue)
                                    {
                                        continue;
                                    }

                                    if (extraChargePeriodsRequired > 0)
                                    {
                                        if (buckets.TakeSlot(targetPoint.IncomingRate, efficiency))
                                        {
                                            _logger.LogInformation($"Setting period {targetPoint.Time} to charge");
                                            targetPoint.ControlAction = ControlAction.Charge;
                                            extraChargePeriodsRequired--;
                                        }
                                        else
                                        {
                                            _logger.LogInformation($"Period {targetPoint.Time} is too expensive to charge");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        // find the points from now backwards, just the points that don't already have a control action
                        var pointsPrior = allPointsWithoutActualFiguresReversed
                            .Where(x => x.Time <= point.Time)
                            .Where(x => !x.ControlAction.HasValue)
                            .ToList();

                        // now find the points in there with the lowest incoming rate
                        foreach (var targetPoint in pointsPrior.Where(x => x.IncomingRate.HasValue).OrderBy(x => x.IncomingRate))
                        {
                            if (numberOfChargePeriodsRequired > 0)
                            {
                                if (buckets.TakeSlot(targetPoint.IncomingRate, efficiency))
                                {
                                    _logger.LogInformation($"Setting period {targetPoint.Time} to charge");

                                    targetPoint.ControlAction = ControlAction.Charge;
                                    numberOfChargePeriodsRequired--;
                                }
                                else
                                {
                                    _logger.LogInformation($"Period {targetPoint.Time} is too expensive to charge");
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        _currentDataService.RecalculateForecast();
                    }
                }

                lastPoint = point;
            }
            _currentDataService.RecalculateForecast();

            void RunPass(string passName, List<TimeSeriesPoint> points, Func<TimeSeriesPoint, bool> selector)
            {
                lastPoint = points.First();
                foreach (var point in points.Skip(1))
                {
                    if (lastPoint.ControlAction == ControlAction.Discharge &&
                        point.ControlAction != ControlAction.Discharge &&
                        lastPoint.RequiredBatteryPowerKwh.HasValue)
                    {
                        _logger.LogInformation($"({passName}) Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh");
                        var lastPointPercent =
                            (((lastPoint.RequiredBatteryPowerKwh.Value / efficiency) / capacity) * 100) +
                            _currentDataService.CurrentState.BatteryReserve;
                        if (lastPointPercent > 100)
                        {
                            lastPointPercent = 100;
                        }
                        _logger.LogInformation($"({passName}) Point at {lastPoint.Time} requires {lastPointPercent:N1}% battery");

                        if (lastPoint.ForecastBatteryPercentage >= lastPointPercent)
                        {
                            _logger.LogInformation($"({passName}) Point at {lastPoint.Time} has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery, no charge needed");
                        }
                        else
                        {
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

                                var allPointsAfterMax = points
                                    .Where(x => x.Time <= point.Time)
                                    .TakeWhile(x => x.ForecastBatteryPercentage < 100)
                                    .Where(selector)
                                    .Where(x => !x.IsDischargeTarget)
                                    .OrderBy(x => x.IncomingRate)
                                    .Take(1)
                                    .ToList();

                                if (!allPointsAfterMax.Any())
                                {
                                    break;
                                }

                                if (!buckets.Any())
                                {
                                    break;
                                }

                                // check if the point we're transforming has a rate low enough to make 
                                // it worth charging
                                var selectedPoint = allPointsAfterMax[0];
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

            // TODO - this section is similar to 'adapt' above

            // now we make a second pass on the discharge points - find any shortfalls and try to fill them in from points that don't have a current control action
            RunPass("Second pass", allPointsWithoutActualFiguresReversed, x => !x.ControlAction.HasValue);

            // now we make a third pass - the point of this pass is to turn any discharge slots to charge if we have a shortfall somewhere
            RunPass("Third pass", allPointsWithoutActualFiguresReversed, x => x.ControlAction != ControlAction.Charge);

            // TODO - we could, for each remaining point, order them by cost. If there is a chance for us to
            // discharge at higher cost and recharge at lower cost, we should do that. This should take into
            // account whether the pair of points being targeted are split by any sections where the battery
            // is already at 100%

            // now set all remaining points to discharge if there is excess power,
            // or hold if there is not
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
                    if (allPointsWithoutActualFigures.Any(x => x.Time > point.Time && x.ControlAction.HasValue))
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
            }

            _currentDataService.RecalculateForecast();
        }
    }
}
