using Solarverse.Core.Data;
using Solarverse.Core.Helper;

namespace Solarverse.Core.Control
{
    public class ControlPlanExecutor : IControlPlanExecutor
    {
        private readonly IInverterActionProvider _inverterActionProvider;
        private readonly ICurrentDataService _currentDataService;

        public ControlPlanExecutor(IInverterActionProvider inverterActionProvider, ICurrentDataService currentDataService)
        {
            _inverterActionProvider = inverterActionProvider;
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

            var currentState = _currentDataService.CurrentState;
            if ((currentPeriod - currentState.UpdateTime).TotalMinutes < 5)
            {
                // if the current inverter state is recent enough
                if (currentState.CurrentAction == currentDataPoint.ControlAction.Value)
                {
                    // if the current inverter action is already what we want, do nothing
                    return true;
                }
            }

            // TODO - work out how long the current state lasts
            var endTime = currentPeriod.AddMinutes(30);

            switch (currentState.CurrentAction)
            {
                case ControlAction.Charge:
                    await _inverterActionProvider.ChargeUntil(endTime);
                    break;
                case ControlAction.Hold:
                    await _inverterActionProvider.Hold();
                    break;
                case ControlAction.Discharge:
                    await _inverterActionProvider.Discharge();
                    break;
                case ControlAction.Export:
                    await _inverterActionProvider.ExportUntil(endTime);
                    break;
            }

            return true;
        }
    }
}
