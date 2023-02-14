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

        public ControlPlanExecutor(IInverterClient inverterClient, ICurrentDataService currentDataService, ILogger<ControlPlanExecutor> logger)
        {
            _inverterClient = inverterClient;
            _currentDataService = currentDataService;
            _logger = logger;
        }

        public async Task<bool> ExecutePlan()
        {
            _logger.LogInformation("Executing control plan...");

            var currentPeriod = Period.HalfHourly.GetLast(DateTime.UtcNow);
            if (!_currentDataService.TimeSeries.TryGetDataPointFor(currentPeriod, out var currentDataPoint) ||
                currentDataPoint?.ControlAction == null)
            {
                // no data point or control action for the current time
                _logger.LogWarning("No data point or control action for the current time.");
                return true;
            }

            _logger.LogInformation("Finding end time for current control state...");

            var endTime = currentPeriod;
            var action = currentDataPoint.ControlAction.Value;
            while (currentDataPoint?.ControlAction == action)
            {
                if (endTime.AddMinutes(30).Date != endTime.Date)
                {
                    // if we would cross a date boundary, then come back to 1 sec
                    // before midnight
                    endTime = endTime.AddMinutes(30).Date.AddSeconds(-1);
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
