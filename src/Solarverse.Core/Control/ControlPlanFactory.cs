using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;

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
                    _currentDataService.TimeSeries.Set(point.Time, x => x.ControlAction = ControlAction.Discharge);
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

            // now create a first pass 'projected battery charge' that just accounts for excess solar
            var allPoints = allPointsReversed.AsEnumerable().Reverse().ToList();
            var capacity = ConfigurationProvider.Configuration.Battery.CapacityKwh ?? 5;
            var efficiency = ConfigurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.92;
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
                        var maxCharge = _currentDataService.CurrentState.MaxChargeRateKw * 0.5;
                        currentPointCharge = maxCharge * efficiency;
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
                if (point.ControlAction != ControlAction.Discharge)
                {
                    var lastPointPercent = 
                        (((lastPoint.RequiredBatteryPowerKwh / efficiency) / capacity) * 100) +
                        _currentDataService.CurrentState.BatteryReserve;
                    if (point.ForecastBatteryPercentage < lastPointPercent)
                    {
                        point.ControlAction = ControlAction.Charge;
                        var maxCharge = _currentDataService.CurrentState.MaxChargeRateKw * 0.5 * efficiency;
                        point.RequiredBatteryPowerKwh = lastPoint.RequiredBatteryPowerKwh - maxCharge;
                        RecalculateForecast(allPoints);
                    }
                }

                lastPoint = point;
            }

            // now set all remaining points to discharge
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
                    point.ControlAction = ControlAction.Discharge;
                }
            }

            RecalculateForecast(allPoints);
        }
    }
}
