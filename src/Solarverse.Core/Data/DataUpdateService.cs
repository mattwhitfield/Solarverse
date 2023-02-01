using Solarverse.Core.Helper;
using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public class CurrentDataService : ICurrentDataService
    {
        public TimeSeries TimeSeries { get; } = new TimeSeries();

        public InverterCurrentState CurrentState { get; private set; } = InverterCurrentState.Default;

        public event EventHandler<EventArgs>? TimeSeriesUpdated;

        public event EventHandler<EventArgs>? CurrentStateUpdated;

        public void Cull(TimeSpan deleteOlderThan)
        {
            var pointsRemoved = TimeSeries.Cull(deleteOlderThan);
            if (pointsRemoved)
            {
                TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public IDisposable LockForUpdate()
        {
            return new CurrentDataLock(this);
        }

        public void Update(InverterCurrentState currentState)
        {
            CurrentState = currentState;
            CurrentStateUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(PredictedConsumption consumption)
        {
            if (!consumption.DataPoints.Any())
            {
                return;
            }

            // a little hack to make the first 'predicted' consumption point line up with the 'actual' graph
            var min = consumption.DataPoints.Min(x => x.Time);
            if (TimeSeries.TryGetDataPointFor(min, out var point) && point != null)
            {
                consumption.DataPoints.Where(x => x.Time == min).Each(x => x.Consumption = point.ActualConsumptionKwh.GetValueOrDefault(x.Consumption));
            }

            TimeSeries.Set(x => x.ForecastConsumptionKwh = null);
            TimeSeries.AddPointsFrom(consumption.DataPoints, x => x.Time, x => x.Consumption, (val, pt) => pt.ForecastConsumptionKwh = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(HouseholdConsumption consumption)
        {
            var min = TimeSeries.GetMinimumDate();

            // TODO - testing
            //var cutoff = new DateTime(2023, 1, 31, 18, 30, 0);
            //var source = consumption.DataPoints.Where(x => x.Time < cutoff);
            var source = consumption.DataPoints;

            TimeSeries.AddPointsFrom(source.Where(x => x.Time >= min), x => x.Time, x => x.Consumption, (val, pt) => pt.ActualConsumptionKwh = val);
            TimeSeries.AddPointsFrom(source.Where(x => x.Time >= min), x => x.Time, x => x.Solar, (val, pt) => pt.ActualSolarKwh = val);
            TimeSeries.AddPointsFrom(source.Where(x => x.Time >= min), x => x.Time, x => x.BatteryPercentage, (val, pt) => pt.ActualBatteryPercentage = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(SolarForecast forecast)
        {
            if (forecast.IsValid)
            {
                TimeSeries.AddPointsFrom(forecast.DataPoints, x => x.Time, x => x.PVEstimate, (val, pt) => pt.ForecastSolarKwh = val);
                TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateIncomingRates(IList<TariffRate> incomingRates)
        {
            TimeSeries.AddPointsFrom(incomingRates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.IncomingRate = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOutgoingRates(IList<TariffRate> outgoingRates)
        {
            TimeSeries.AddPointsFrom(outgoingRates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.OutgoingRate = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void RecalculateForecast()
        {
            var efficiency = ConfigurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            var maxChargeKwhPerPeriod = CurrentState.MaxChargeRateKw * 0.5 * efficiency;
            var capacity = ConfigurationProvider.Configuration.Battery.CapacityKwh ?? 5;

            var lastActual = TimeSeries.OrderBy(x => x.Time).LastOrDefault(x => x.ActualBatteryPercentage.HasValue);
            var lastPercentage = lastActual?.ActualBatteryPercentage ?? 4;

            foreach (var point in TimeSeries.Where(x => !x.ActualBatteryPercentage.HasValue).OrderBy(x => x.Time))
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
                    var maxDischarge = CurrentState.MaxDischargeRateKw * 0.5;
                    currentPointDischarge = maxDischarge * efficiency;
                }
                else if (point.ControlAction == ControlAction.Charge)
                {
                    currentPointCharge = maxChargeKwhPerPeriod;
                }
                var currentPointPercent = ((currentPointCharge - currentPointDischarge) / capacity) * 100;
                var thisPercentage = lastPercentage + currentPointPercent;

                if (thisPercentage < CurrentState.BatteryReserve)
                {
                    thisPercentage = CurrentState.BatteryReserve;
                }
                if (thisPercentage > 100)
                {
                    thisPercentage = 100;
                }
                point.ForecastBatteryPercentage = lastPercentage;
                lastPercentage = thisPercentage;
            }
        }

        private class CurrentDataLock : IDisposable
        {
            private readonly CurrentDataService _currentDataService;

            public CurrentDataLock(CurrentDataService currentDataService)
            {
                _currentDataService = currentDataService;
                GC.SuppressFinalize(this);
            }

            public void Dispose()
            {
                _currentDataService.TimeSeriesUpdated?.Invoke(_currentDataService, EventArgs.Empty);
            }
        }
    }
}
