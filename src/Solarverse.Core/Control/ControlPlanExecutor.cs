using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;

namespace Solarverse.Core.Control
{
    public class ControlPlanExecutor : IControlPlanExecutor
    {
        private readonly IInverterClient _inverterClient;
        private readonly ICurrentDataService _currentDataService;
        private readonly ILogger<ControlPlanExecutor> _logger;
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private readonly IEVChargerClient _evChargerClient;

        public ControlPlanExecutor(IInverterClient inverterClient, ICurrentDataService currentDataService, ILogger<ControlPlanExecutor> logger, ICurrentTimeProvider currentTimeProvider, IEVChargerClient evChargerClient)
        {
            _inverterClient = inverterClient;
            _currentDataService = currentDataService;
            _logger = logger;
            _currentTimeProvider = currentTimeProvider;
            _evChargerClient = evChargerClient;
        }

        public async Task<bool> ExecutePlan()
        {
            _logger.LogInformation("Executing control plan...");

            var currentPeriod = _currentTimeProvider.CurrentPeriodStartUtc;
            if (!_currentDataService.TimeSeries.TryGetDataPointFor(currentPeriod, out var currentDataPoint))
            {
                _logger.LogWarning("No data point for the current time.");
                return true;
            }

            if (_currentDataService.TimeSeries.TryGetDataPointFor(currentPeriod.Add(TimeSpan.FromMinutes(-30)), out var lastDataPoint))
            {
                _logger.LogInformation($"Found prior data point - ShouldChargeEV {lastDataPoint.ShouldChargeEV} -> {currentDataPoint.ShouldChargeEV}...");

                if (lastDataPoint.ShouldChargeEV != currentDataPoint.ShouldChargeEV)
                {
                    _logger.LogInformation($"EV Charge status changing to {currentDataPoint.ShouldChargeEV}...");
                    await _evChargerClient.SetChargingEnabled(currentDataPoint.ShouldChargeEV);
                }
                else
                {
                    _logger.LogInformation("No change in EV Charge status");
                }
            }

            if (currentDataPoint?.ControlAction == null)
            {
                // no data point or control action for the current time
                _logger.LogWarning("No data point or control action for the current time.");
                return true;
            }

            _logger.LogInformation("Finding end time for current control state...");

            Func<DateTime, DateTime> transformTime = x => x;

            // if the inverter uses local time, then we need to transform the date to local time when evaluating boundaries
            if (_inverterClient.UsesLocalTimeBoundary)
            {
                transformTime = x => _currentTimeProvider.ToLocalTime(x);
            }

            var endTime = currentPeriod;
            var action = currentDataPoint.ControlAction.Value;
            while (currentDataPoint?.ControlAction == action)
            {
                if (transformTime(endTime.AddMinutes(30)).Date != transformTime(endTime).Date)
                {
                    // if we would cross a date boundary, then come back to 1 sec
                    // before where the boundary is
                    endTime = endTime.AddMinutes(30).AddSeconds(-1);
                    break;
                }

                endTime = endTime.AddMinutes(30);
                if (!_currentDataService.TimeSeries.TryGetDataPointFor(endTime, out currentDataPoint))
                {
                    // no data point or control action for the current time
                    break;
                }
            }

            _logger.LogInformation($"Control action {action} ends at {endTime}");

            switch (action)
            {
                case ControlAction.Charge:
                    await _inverterClient.Charge(endTime);
                    break;
                case ControlAction.Hold:
                    await _inverterClient.Hold(endTime);
                    break;
                case ControlAction.Discharge:
                    await _inverterClient.Discharge(endTime);
                    break;
                case ControlAction.Export:
                    await _inverterClient.Export(endTime);
                    break;
            }

            return true;
        }
    }
}
