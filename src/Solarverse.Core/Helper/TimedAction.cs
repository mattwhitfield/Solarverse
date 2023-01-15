using Microsoft.Extensions.Logging;

namespace Solarverse.Core.Helper
{
    public class TimedAction
    {
        private readonly ILogger _logger;
        private readonly Period _period;
        private DateTime _due;
        private DateTime _lastSuccessfulDue;
        private readonly Func<Task<bool>> _execute;
        private readonly string _actionName;

        public TimedAction(
            ILogger logger,
            Period period,
            Func<Task<bool>> execute,
            string actionName)
        {
            _logger = logger;
            _period = period;
            _lastSuccessfulDue = _due = _period.GetLast(DateTime.UtcNow);
            _execute = execute;
            _actionName = actionName;
        }

        public async Task<TimedActionResult> Run(DateTime current)
        {
            if (current > _due)
            {
                _logger.LogInformation($"Action {_actionName} due, executing...");
                using var scope = _logger.BeginScope("Action {action} running", _actionName);
                bool succeeded;

                try
                {
                    succeeded = await _execute();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while executing {_actionName}");
                    succeeded = false;
                }

                if (succeeded)
                {
                    _lastSuccessfulDue = _due = _period.GetNext(_lastSuccessfulDue);
                    _logger.LogInformation($"Action {_actionName} succeeded, next due at {_due}");
                    return TimedActionResult.Succeeded;
                }
                else
                {
                    _due = _due.AddSeconds(15);
                    _logger.LogInformation($"Action {_actionName} failed, next due at {_due}");
                    return TimedActionResult.Failed;
                }
            }

            return TimedActionResult.NotDue;
        }

    }
}
