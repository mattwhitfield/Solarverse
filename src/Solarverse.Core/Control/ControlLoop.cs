using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;
using Solarverse.Core.Models;

namespace Solarverse.Core.Control
{
    public class ControlLoop : IControlLoop
    {
        private readonly ILogger<ControlLoop> _logger;
        private readonly IDataStore _dataStore;
        private readonly IInverterClient _inverterClient;
        private readonly ICurrentDataService _dataUpdateService;
        private readonly IControlPlanFactory _controlPlanFactory;
        private readonly IControlPlanExecutor _controlPlanExecutor;

        private readonly List<TimedAction> _actions = new List<TimedAction>();

        public ControlLoop(ILogger<ControlLoop> logger,
            IDataStore dataStore,
            IInverterClient inverterClient,
            ICurrentDataService dataUpdateService,
            IControlPlanFactory controlPlanFactory,
            IControlPlanExecutor controlPlanExecutor)
        {
            _logger = logger;
            _dataStore = dataStore;
            _inverterClient = inverterClient;
            _dataUpdateService = dataUpdateService;
            _controlPlanFactory = controlPlanFactory;
            _controlPlanExecutor = controlPlanExecutor;

            var getTariffRatesPeriod = UpdatePeriods.TariffUpdates;
            _actions.Add(new TimedAction(_logger, getTariffRatesPeriod, UpdateTariffRates, "Update energy tariff rates"));

            var getSolarForecastDataPeriod = UpdatePeriods.SolarForecastUpdates;
            _actions.Add(new TimedAction(_logger, getSolarForecastDataPeriod, UpdateSolcastData, "Update solar forecast data"));

            var currentStatusPeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromMinutes(29));
            _actions.Add(new TimedAction(_logger, currentStatusPeriod, UpdateCurrentStatus, "Update current inverter status"));

            var executePeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromSeconds(15));
            _actions.Add(new TimedAction(_logger, executePeriod, ExecuteControlPlan, "Execute control plan"));

            var dataCleanupPeriod = new Period(TimeSpan.FromHours(0.5));
            _actions.Add(new TimedAction(_logger, dataCleanupPeriod, CleanUpData, "Cleaning up old data"));
        }

        public async Task Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                foreach (var action in _actions)
                {
                    await action.Run(DateTime.UtcNow);

                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                }

                await Task.Delay(1000, cancellation);
            }
        }

        public async Task<bool> UpdateCurrentStatus()
        {
            var currentState = await _inverterClient.GetCurrentState();

            if (currentState != null)
            {
                _dataUpdateService.Update(currentState);
                _controlPlanFactory.CheckForAdaptations(currentState);

                return true;
            }

            return false;
        }

        public Task<bool> CleanUpData()
        {
            _dataUpdateService.Cull(TimeSpan.FromDays(7));
            return Task.FromResult(true);
        }

        public Task<bool> ExecuteControlPlan()
        {
            return _controlPlanExecutor.ExecutePlan();
        }

        public async Task<bool> UpdateTariffRates()
        {
            var incoming = ConfigurationProvider.Configuration.IncomingMeter;
            var outgoing = ConfigurationProvider.Configuration.OutgoingMeter;

            var succeeded =
                await UpdateTariffRates(incoming, _dataUpdateService.UpdateIncomingRates) &&
                await UpdateTariffRates(outgoing, _dataUpdateService.UpdateOutgoingRates);

            if (succeeded)
            {
                _controlPlanFactory.CreatePlan();
            }

            return succeeded;
        }

        private async Task<bool> UpdateTariffRates(MeterPointConfiguration? meter, Action<IList<TariffRate>> process)
        {
            if (meter?.TariffName == null || meter.MPAN == null)
            {
                return true;
            }

            var agileRates = await _dataStore.GetTariffRates(
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
            var forecast = await _dataStore.GetSolarForecast();

            if (forecast != null)
            {
                _dataUpdateService.Update(forecast);
                return true;
            }

            return false;
        }
    }
}
