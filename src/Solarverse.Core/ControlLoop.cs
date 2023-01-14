using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.Octopus.Models;
using Solarverse.Core.Services;

namespace Solarverse.Core
{
    public class ControlLoop
    {
        private readonly ILogger<ControlLoop> _logger;
        private readonly IIntegrationProvider _integrationProvider;
        private readonly IDataUpdateService _dataUpdateService;
        private readonly IControlPlanFactory _controlPlanFactory;
        private readonly IControlPlanExecutor _controlPlanExecutor;

        private TimedAction _currentStatus;
        private TimedAction _getAgileRates;
        private TimedAction _getSolcastData;
        //private readonly TimedAction 

        public ControlLoop(ILogger<ControlLoop> logger,
            IIntegrationProvider integrationProvider,
            IDataUpdateService dataUpdateService,
            IControlPlanFactory controlPlanFactory,
            IControlPlanExecutor controlPlanExecutor)
        {
            _logger = logger;
            _integrationProvider = integrationProvider;
            _dataUpdateService = dataUpdateService;
            _controlPlanFactory = controlPlanFactory;
            _controlPlanExecutor = controlPlanExecutor;

            var currentStatusPeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromMinutes(29));
            _currentStatus = new TimedAction(_logger, currentStatusPeriod, UpdateCurrentStatus, "Update current status");

            var getAgileRatesPeriod = new Period(TimeSpan.FromDays(1), TimeSpan.FromHours(17));
            _getAgileRates = new TimedAction(_logger, getAgileRatesPeriod, UpdateAgileRates, "Update agile rates");

            var getSolcastDataPeriod = new Period(TimeSpan.FromHours(6), TimeSpan.Zero);
            _getSolcastData = new TimedAction(_logger, getSolcastDataPeriod, UpdateSolcastData, "Update Solcast data");
        }

        public async Task Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await _currentStatus.Run(DateTime.UtcNow);
                await _getAgileRates.Run(DateTime.UtcNow);
                await _getSolcastData.Run(DateTime.UtcNow);

                await Task.Delay(1000);
            }
        }

        public async Task<bool> UpdateCurrentStatus()
        {
            // todo
            var currentState = await _integrationProvider.GivEnergyClient.GetCurrentState();

            if (currentState != null)
            {
                _dataUpdateService.Update(currentState);
                _controlPlanFactory.CheckForAdaptations(currentState);

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateAgileRates()
        {
            var incoming = ConfigurationProvider.Configuration.IncomingMeter;
            var outgoing = ConfigurationProvider.Configuration.OutgoingMeter;

            var succeeded =
                await UpdateAgileRates(incoming, _dataUpdateService.UpdateIncomingRates) &&
                await UpdateAgileRates(outgoing, _dataUpdateService.UpdateOutgoingRates);

            if (succeeded)
            {
                _controlPlanFactory.CreatePlan();
            }

            return false;
        }

        private async Task<bool> UpdateAgileRates(Models.MeterPoint? meter, Action<AgileRates> process)
        {
            if (meter?.TariffName == null || meter.MPAN == null)
            {
                return true;
            }

            var agileRates = await _integrationProvider.OctopusClient.GetAgileRates(
                meter.TariffName, meter.MPAN);

            if (agileRates != null)
            {
                process(agileRates);
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateSolcastData()
        {
            var siteId = ConfigurationProvider.Configuration.SolcastSiteId;
            var forecast = await _integrationProvider.SolcastClient.GetForecastSet(siteId);

            if (forecast != null)
            {
                _dataUpdateService.Update(forecast);
                return true;
            }

            return false;
        }
    }
}
