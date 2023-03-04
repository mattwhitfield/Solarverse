using Solarverse.Core.Control;
using Solarverse.Core.Data;

namespace Solarverse.Server
{
    public class ControlLoopService : BackgroundService
    {
        private readonly IControlLoop _controlLoop;
        private readonly IDataStore _dataStore;
        private readonly ICurrentDataService _currentDataService;

        public ControlLoopService(IControlLoop controlLoop, IDataStore dataStore, ICurrentDataService currentDataService)
        {
            _controlLoop = controlLoop;
            _dataStore = dataStore;
            _currentDataService = currentDataService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _controlLoop.Run(stoppingToken);
        }
    }
}
