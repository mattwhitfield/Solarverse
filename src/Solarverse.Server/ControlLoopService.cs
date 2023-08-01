using Solarverse.Core.Control;

namespace Solarverse.Server
{
    public class ControlLoopService : BackgroundService
    {
        private readonly IControlLoop _controlLoop;

        public ControlLoopService(IControlLoop controlLoop)
        {
            _controlLoop = controlLoop;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _controlLoop.Run(stoppingToken);
        }
    }
}
