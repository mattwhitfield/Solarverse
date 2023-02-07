using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System.Drawing;
using System.Text;

namespace Solarverse.Core.Data
{
    public class CurrentDataService : ICurrentDataService
    {
        private readonly ILogger<CurrentDataService> _logger;

        public CurrentDataService(ILogger<CurrentDataService> logger)
        {
            _logger = logger;
        }

        public TimeSeries TimeSeries { get; } = new TimeSeries();

        public InverterCurrentState CurrentState { get; private set; } = InverterCurrentState.Default;

        public event EventHandler<EventArgs>? TimeSeriesUpdated;

        public event EventHandler<EventArgs>? CurrentStateUpdated;

        public void Cull(TimeSpan deleteOlderThan)
        {
            _logger.LogInformation($"Culling time series data older than {deleteOlderThan}");

            var pointsRemoved = TimeSeries.Cull(deleteOlderThan);
            if (pointsRemoved)
            {
                TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public IDisposable LockForUpdate()
        {
            _logger.LogInformation($"Locking time series for update");
            return new CurrentDataLock(this);
        }

        public void Update(InverterCurrentState currentState)
        {
            _logger.LogInformation($"Updating current inverter state");
            CurrentState = currentState;
            CurrentStateUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(PredictedConsumption consumption)
        {
            _logger.LogInformation($"Updating predicted consumption");
            if (!consumption.DataPoints.Any())
            {
                _logger.LogWarning($"No predicted consumption data points to update");
                return;
            }

            TimeSeries.Set(x => x.ForecastConsumptionKwh = null);
            TimeSeries.AddPointsFrom(consumption.DataPoints, x => x.Time, x => x.Consumption, (val, pt) => pt.ForecastConsumptionKwh = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(HouseholdConsumption consumption)
        {
            _logger.LogInformation($"Updating household consumption");

            var min = TimeSeries.GetMinimumDate();
            var max = new Period(TimeSpan.FromMinutes(30)).GetLast(DateTime.UtcNow);
            _logger.LogInformation($"Minimum household consumption time is {min}");

            var source = consumption.DataPoints;

            TimeSeries.AddPointsFrom(source.Where(x => x.Time >= min && x.Time < max), x => x.Time, x => x.Consumption, (val, pt) => pt.ActualConsumptionKwh = val);
            TimeSeries.AddPointsFrom(source.Where(x => x.Time >= min && x.Time < max), x => x.Time, x => x.Solar, (val, pt) => pt.ActualSolarKwh = val);
            TimeSeries.AddPointsFrom(source.Where(x => x.Time >= min), x => x.Time, x => x.BatteryPercentage, (val, pt) => pt.ActualBatteryPercentage = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(SolarForecast forecast)
        {
            _logger.LogInformation($"Updating solar forecast");
            if (forecast.IsValid)
            {
                _logger.LogInformation($"Solar forecast is valid");

                TimeSeries.AddPointsFrom(forecast.DataPoints, x => x.Time, x => x.PVEstimate, (val, pt) => pt.ForecastSolarKwh = val);
                TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateIncomingRates(IList<TariffRate> incomingRates)
        {
            _logger.LogInformation($"Updating incoming rates");
            TimeSeries.AddPointsFrom(incomingRates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.IncomingRate = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOutgoingRates(IList<TariffRate> outgoingRates)
        {
            _logger.LogInformation($"Updating outgoing rates");
            TimeSeries.AddPointsFrom(outgoingRates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.OutgoingRate = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void RecalculateForecast()
        {
            _logger.LogInformation($"Recalculating forecast");
            var efficiency = ConfigurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            var maxChargeKwhPerPeriod = CurrentState.MaxChargeRateKw * 0.5 * efficiency;
            var capacity = ConfigurationProvider.Configuration.Battery.CapacityKwh ?? 5;

            var lastActual = TimeSeries.OrderBy(x => x.Time).LastOrDefault(x => x.ActualBatteryPercentage.HasValue);
            var lastPercentage = lastActual?.ActualBatteryPercentage ?? 4;

            _logger.LogInformation($"Forecast start - efficiency = {efficiency:N2}, maxChargeKwhPerPeriod = {maxChargeKwhPerPeriod:N2} kWh, capacity = {capacity:N2} kWh, lastPercentage = {lastPercentage:N1}%");

            var logBuilder = new StringBuilder();

            foreach (var point in TimeSeries.Where(x => !x.ActualBatteryPercentage.HasValue && x.IncomingRate.HasValue).OrderBy(x => x.Time))
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

                if (logBuilder.Length > 0)
                {
                    logBuilder.Append(", ");
                }
                logBuilder.Append(point.Time.ToString("HH:mm")).Append("=").Append(lastPercentage.ToString("N1")).Append("%");

                point.ForecastBatteryPercentage = lastPercentage;
                lastPercentage = thisPercentage;
            }

            _logger.LogInformation($"Forecast = {logBuilder}");
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
                _currentDataService._logger.LogInformation($"Unlocking time series and notifying update");
                _currentDataService.TimeSeriesUpdated?.Invoke(_currentDataService, EventArgs.Empty);
            }
        }
    }
}
