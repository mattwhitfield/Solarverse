using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using Solarverse.Core.Services;

namespace Solarverse.Core
{
    public class ControlLoop
    {
        private readonly ILogger<ControlLoop> _logger;
        private readonly IDataUpdateService _dataUpdateService;
        private readonly IControlPlanFactory _controlPlanFactory;
        private readonly IControlPlanExecutor _controlPlanExecutor;
        private TimedAction _currentStatus;
        //private readonly TimedAction 

        public ControlLoop(ILogger<ControlLoop> logger,
            IDataUpdateService dataUpdateService,
            IControlPlanFactory controlPlanFactory,
            IControlPlanExecutor controlPlanExecutor)
        {
            _logger = logger;
            _dataUpdateService = dataUpdateService;
            _controlPlanFactory = controlPlanFactory;
            _controlPlanExecutor = controlPlanExecutor;

            var currentStatusPeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromMinutes(29));
            _currentStatus = new TimedAction(_logger, currentStatusPeriod, UpdateCurrentStatus, "Update current status");

            var getAgileRatesPeriod = new Period(TimeSpan.FromDays(1), TimeSpan.FromHours(17));
            _getAgileRates = new TimedAction(_logger, getAgileRatesPeriod, UpdateAgileRates, "Update agile rates");
        }

        public async Task Run(CancellationToken cancellation)
        {
            
        }

        public async Task<bool> UpdateCurrentStatus()
        {
            // todo

            return true;
        }

        public async Task<bool> UpdateAgileRates()
        {
            // todo

            return true;
        }
    }
}
