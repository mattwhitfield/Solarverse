using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Data.Prediction;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;
using Solarverse.Core.Models;
using Solarverse.Core.Models.Settings;

namespace Solarverse.Core.Control
{
    public class ControlLoop : IControlLoop
    {
        private readonly ILogger<ControlLoop> _logger;
        private readonly IDataStore _dataStore;
        private readonly IInverterClient _inverterClient;
        private readonly ICurrentDataService _currentDataService;
        private readonly IControlPlanFactory _controlPlanFactory;
        private readonly IControlPlanExecutor _controlPlanExecutor;
        private readonly IPredictionFactory _predictionFactory;

        private readonly List<TimedAction> _actions = new List<TimedAction>();

        public ControlLoop(ILogger<ControlLoop> logger,
            IDataStore dataStore,
            IInverterClient inverterClient,
            ICurrentDataService currentDataService,
            IControlPlanFactory controlPlanFactory,
            IControlPlanExecutor controlPlanExecutor,
            IPredictionFactory predictionFactory)
        {
            _logger = logger;
            _dataStore = dataStore;
            _inverterClient = inverterClient;
            _currentDataService = currentDataService;
            _controlPlanFactory = controlPlanFactory;
            _controlPlanExecutor = controlPlanExecutor;
            _predictionFactory = predictionFactory;

            var getSolarForecastDataPeriod = UpdatePeriods.SolarForecastUpdates;
            _actions.Add(new TimedAction(_logger, getSolarForecastDataPeriod, UpdateSolcastData, "Update solar forecast data"));

            var getTariffRatesPeriod = UpdatePeriods.TariffUpdates;
            _actions.Add(new TimedAction(_logger, getTariffRatesPeriod, UpdateTariffRates, "Update energy tariff rates"));

            var consumptionUpdatePeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromMinutes(1));
            _actions.Add(new TimedAction(_logger, consumptionUpdatePeriod, GetConsumptionData, "Get consumption data"));

            var planUpdatePeriod = new Period(TimeSpan.FromHours(0.5));
            _actions.Add(new TimedAction(_logger, planUpdatePeriod, UpdatePlan, "Update control plan"));

            // TODO - re-enable
            //var currentStatusPeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromMinutes(29));
            //_actions.Add(new TimedAction(_logger, currentStatusPeriod, UpdateCurrentStatus, "Update current inverter status"));

            //var executePeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromSeconds(15));
            //_actions.Add(new TimedAction(_logger, executePeriod, ExecuteControlPlan, "Execute control plan"));

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

                try
                {
                    await Task.Delay(1000, cancellation);
                }
                catch (OperationCanceledException)
                { }
            }
        }

        public async Task<bool> UpdateCurrentStatus()
        {
            var currentState = await _inverterClient.GetCurrentState();

            if (currentState != null)
            {
                _currentDataService.Update(currentState);
                _controlPlanFactory.CheckForAdaptations(currentState);

                return true;
            }

            return false;
        }

        public Task<bool> CleanUpData()
        {
            _currentDataService.Cull(TimeSpan.FromDays(7));
            return Task.FromResult(true);
        }

        public Task<bool> ExecuteControlPlan()
        {
            return _controlPlanExecutor.ExecutePlan();
        }

        public async Task<bool> GetConsumptionData()
        {
            bool anyFailed = false;
            var timeSeries = _currentDataService.TimeSeries;

            foreach (var date in timeSeries.GetDates().Where(x => x.Date <= DateTime.UtcNow.Date))
            {
                var data = await _dataStore.GetHouseholdConsumptionFor(date);
                if (data != null)
                {
                    _currentDataService.Update(data);
                }
                else
                {
                    anyFailed = true;
                }
            }

            var from = timeSeries.GetMaximumDate(x => x.ActualConsumptionKwh != null);
            var to = timeSeries.GetMaximumDate();

            if (from.HasValue && to.HasValue)
            {
                var aggregateConsumption = await _predictionFactory.CreatePredictionFrom(from.Value, to.Value);
                _currentDataService.Update(aggregateConsumption);
            }

            return !anyFailed;
        }

        public async Task<bool> UpdateTariffRates()
        {
            var incoming = ConfigurationProvider.Configuration.IncomingMeter;
            var outgoing = ConfigurationProvider.Configuration.OutgoingMeter;

            var succeeded =
                await UpdateTariffRates(incoming, _currentDataService.UpdateIncomingRates) &&
                await UpdateTariffRates(outgoing, _currentDataService.UpdateOutgoingRates);

            return succeeded;
        }

        public Task<bool> UpdatePlan()
        {
            if (_currentDataService.TimeSeries.Any(x => x.IncomingRate.HasValue && !x.ControlAction.HasValue))
            {
                _controlPlanFactory.CreatePlan();
            }

            return Task.FromResult(true);
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
                _currentDataService.Update(forecast);
                return true;
            }

            return false;
        }
    }
}
