using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System.Runtime.Serialization;

namespace Solarverse.Core.Control
{
    public class ControlPlanFactory : IControlPlanFactory
    {
        private readonly ICurrentDataService _currentDataService;

        public ControlPlanFactory(ICurrentDataService currentDataService)
        {
            _currentDataService = currentDataService;
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
            var firstTime = new Period(TimeSpan.FromMinutes(30)).GetNext(DateTime.UtcNow);

            using var updateLock = _currentDataService.LockForUpdate();

            _currentDataService.TimeSeries.Set(x => x.RequiredBatteryPowerKwh = null);

            var tariffRates = _currentDataService.TimeSeries
                .GetSeries(x => x.IncomingRate)
                .Where(x => x.Time >= firstTime)
                .ToList();

            var count = tariffRates.Count;
            var ratesByGroup = tariffRates.GroupBy(x => x.Value).OrderByDescending(x => x.Key).ToList();

            var dischargeTimesSet = 0;
            foreach (var rate in ratesByGroup)
            {
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
            var allPointsReversed = _currentDataService.TimeSeries.OrderByDescending(x => x.Time).ToList();
            if (!allPointsReversed.Any())
            {
                return;
            }

            var lastPointPower = 0d;
            foreach (var point in allPointsReversed)
            {
                if (point.ControlAction == ControlAction.Discharge)
                {
                    var pointPower = lastPointPower + (point.RequiredPowerKwh ?? 0);
                    point.RequiredBatteryPowerKwh = lastPointPower = pointPower;                    
                }
                else
                {
                    lastPointPower = 0;
                }

                if (point.IncomingRate.HasValue && point.IncomingRate.Value < 0)
                {
                    point.ControlAction = ControlAction.Charge;
                }
            }

            var efficiency = ConfigurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            var maxChargeKwhPerPeriod = _currentDataService.CurrentState.MaxChargeRateKw * 0.5 * efficiency;

            // now create a first pass 'projected battery charge' that just accounts for excess solar
            var allPoints = allPointsReversed.AsEnumerable().Reverse().ToList();
            var capacity = ConfigurationProvider.Configuration.Battery.CapacityKwh ?? 5;
            var singleDirectionEfficieny = 1 - ((1 - efficiency) / 2);
            void RecalculateForecast(List<TimeSeriesPoint> points)
            {
                var lastActual = points.LastOrDefault(x => x.ActualBatteryPercentage.HasValue);
                var lastPercentage = lastActual?.ActualBatteryPercentage ?? 4;
                foreach (var point in _currentDataService.TimeSeries.Where(x => !x.ActualBatteryPercentage.HasValue).OrderBy(x => x.Time))
                {
                    var currentPointCharge = point.ExcessPowerKwh ?? 0;
                    if (currentPointCharge < 0)
                    {
                        currentPointCharge = 0;
                    }
                    currentPointCharge *= efficiency;

                    var currentPointDischarge = 0d;
                    if (point.ControlAction == ControlAction.Discharge)
                    {
                        currentPointDischarge = point.RequiredPowerKwh ?? 0;
                        if (currentPointDischarge < 0)
                        {
                            currentPointDischarge = 0;
                        }
                        currentPointDischarge /= efficiency;
                    }
                    else if (point.ControlAction == ControlAction.Export)
                    {
                        var maxDischarge = _currentDataService.CurrentState.MaxDischargeRateKw * 0.5;
                        currentPointDischarge = maxDischarge * efficiency;
                    }
                    else if (point.ControlAction == ControlAction.Charge)
                    {
                        currentPointCharge = maxChargeKwhPerPeriod;
                    }
                    var currentPointPercent = ((currentPointCharge - currentPointDischarge) / capacity) * 100;
                    var thisPercentage = lastPercentage + currentPointPercent;

                    if (thisPercentage < _currentDataService.CurrentState.BatteryReserve)
                    {
                        thisPercentage = _currentDataService.CurrentState.BatteryReserve;
                    }
                    if (thisPercentage > 100)
                    {
                        thisPercentage = 100;
                    }
                    point.ForecastBatteryPercentage = lastPercentage; 
                    lastPercentage = thisPercentage;
                }
            }

            RecalculateForecast(allPoints);

            // now go through backwards, where we have transitions from discharge to no plan
            // evaluate the difference between forecast battery percentage and the required 
            // battery percentage

            // TODO - this is overly simplistic - it should go search backwards until the next
            // specified action, and then set charge for the points that are the cheapest
            var lastPoint = allPointsReversed.First();
            foreach (var point in allPointsReversed.Skip(1))
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

                    if (point.ForecastBatteryPercentage < lastPointPercent)
                    {
                        var shortfallPercent = lastPointPercent - (point.ForecastBatteryPercentage ?? 0);
                        var shortfallKwh = capacity * shortfallPercent / 100;

                        var numberOfChargePeriodsRequired = (int)Math.Ceiling(shortfallKwh / maxChargeKwhPerPeriod);

                        // find the points from now backwards, until there is a point
                        // where we don't have enough headroom, and then just the points
                        // that don't already have a control action
                        var pointsPrior = allPointsReversed
                            .Where(x => x.Time <= point.Time)
                            .TakeWhile(x => !x.RequiredBatteryPowerKwh.HasValue || capacity - x.RequiredBatteryPowerKwh.Value >= shortfallKwh)
                            .Where(x => !x.ControlAction.HasValue)
                            .ToList();

                        // now find the points in there with the lowest incoming rate
                        foreach (var targetPoint in pointsPrior.Where(x => x.IncomingRate.HasValue).OrderBy(x => x.IncomingRate))
                        {
                            if (numberOfChargePeriodsRequired > 0)
                            {
                                targetPoint.ControlAction = ControlAction.Charge;
                                numberOfChargePeriodsRequired--;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }


                lastPoint = point;
            }
            RecalculateForecast(allPoints);

            // now set all remaining points to discharge if there is excess power,
            // or hold if there is not
            foreach (var point in allPoints)
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
                    point.ControlAction = point.ExcessPowerKwh.HasValue && point.ExcessPowerKwh.Value > 0 ?
                        ControlAction.Discharge : ControlAction.Hold;
                }
            }

            RecalculateForecast(allPoints);
        }
    }
}
