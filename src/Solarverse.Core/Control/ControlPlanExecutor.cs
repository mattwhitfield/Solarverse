using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;

namespace Solarverse.Core.Control
{
    public class ControlPlanExecutor : IControlPlanExecutor
    {
        private readonly IInverterClient _inverterClient;
        private readonly ICurrentDataService _currentDataService;

        public ControlPlanExecutor(IInverterClient inverterClient, ICurrentDataService currentDataService)
        {
            _inverterClient = inverterClient;
            _currentDataService = currentDataService;
        }

        public async Task<bool> ExecutePlan()
        {
            var currentPeriod = DateTime.UtcNow.ToHalfHourPeriod();
            if (!_currentDataService.TimeSeries.TryGetDataPointFor(currentPeriod, out var currentDataPoint) ||
                currentDataPoint?.ControlAction == null)
            {
                // no data point or control action for the current time
                return true;
            }
           
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
