using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Data.Prediction;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;
using Solarverse.Core.Models;
using Solarverse.Core.Models.Settings;
using System.Diagnostics.Metrics;

namespace Solarverse.Core.Control
{
    public class ControlLoop : IControlLoop
    {
        private readonly ILogger<ControlLoop> _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IDataStore _dataStore;
        private readonly IInverterClient _inverterClient;
        private readonly ICurrentDataService _currentDataService;
        private readonly IControlPlanFactory _controlPlanFactory;
        private readonly IControlPlanExecutor _controlPlanExecutor;
        private readonly IPredictionFactory _predictionFactory;

        private readonly List<TimedAction> _actions = new List<TimedAction>();

        public ControlLoop(ILogger<ControlLoop> logger,
            IConfigurationProvider configurationProvider,
            IDataStore dataStore,
            IInverterClient inverterClient,
            ICurrentDataService currentDataService,
            IControlPlanFactory controlPlanFactory,
            IControlPlanExecutor controlPlanExecutor,
            IPredictionFactory predictionFactory)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
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

            var currentStatusPeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromMinutes(29));
            _actions.Add(new TimedAction(_logger, currentStatusPeriod, UpdateCurrentStatus, "Update current inverter status"));

            if (!_configurationProvider.Configuration.TestMode)
            {
                var executePeriod = new Period(TimeSpan.FromHours(0.5), TimeSpan.FromSeconds(15));
                _actions.Add(new TimedAction(_logger, executePeriod, ExecuteControlPlan, "Execute control plan"));
            }

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
            _logger.LogInformation($"Updating current status...");
            var currentState = await _inverterClient.GetCurrentState();

            if (currentState != null)
            {
                _logger.LogInformation($"Got current status update");
                _currentDataService.Update(currentState);

                _logger.LogInformation($"Checking for plan adaptations");
                _controlPlanFactory.CheckForAdaptations(currentState);

                return true;
            }

            _logger.LogWarning($"Did not get current status update");
            return false;
        }

        public Task<bool> CleanUpData()
        {
            _logger.LogInformation($"Cleaning up old data");
            _currentDataService.Cull(TimeSpan.FromDays(7));
            return Task.FromResult(true);
        }

        public Task<bool> ExecuteControlPlan()
        {
            _logger.LogInformation($"Executing control plan");
            return _controlPlanExecutor.ExecutePlan();
        }

        public async Task<bool> GetConsumptionData()
        {
            _logger.LogInformation($"Getting consumption data");

            bool anyFailed = false;
            var timeSeries = _currentDataService.TimeSeries;

            foreach (var date in timeSeries.GetDates().Where(x => x.Date <= DateTime.UtcNow.Date))
            {
                _logger.LogInformation($"Getting household consumption for {date}");

                var data = await _dataStore.GetHouseholdConsumptionFor(date);
                if (data != null && data.IsValid)
                {
                    _logger.LogInformation($"Got household consumption for {date}");
                    _currentDataService.Update(data);
                }
                else
                {
                    _logger.LogWarning($"Did not get household consumption for {date}");
                    anyFailed = true;
                }
            }

            var from = timeSeries.GetMaximumDate(x => x.ActualConsumptionKwh != null);
            var to = timeSeries.GetMaximumDate();

            if (from.HasValue && to.HasValue)
            {
                _logger.LogInformation($"Time series range after actual figures - from {from} to {to}, creating prediction");
                var aggregateConsumption = await _predictionFactory.CreatePredictionFrom(from.Value, to.Value);
                _currentDataService.Update(aggregateConsumption);
            }

            return !anyFailed;
        }

        public async Task<bool> UpdateTariffRates()
        {
            _logger.LogInformation($"Updating tariff rates");
            var incoming = _configurationProvider.Configuration.IncomingMeter;
            var outgoing = _configurationProvider.Configuration.OutgoingMeter;

            var succeeded =
                await UpdateTariffRates(incoming, _currentDataService.UpdateIncomingRates) &&
                await UpdateTariffRates(outgoing, _currentDataService.UpdateOutgoingRates);

            return succeeded;
        }

        public Task<bool> UpdatePlan()
        {
            _logger.LogInformation($"Updating plan");
            if (_currentDataService.TimeSeries.Any(x => x.IncomingRate.HasValue && !x.ControlAction.HasValue && !x.ActualConsumptionKwh.HasValue))
            {
                _logger.LogInformation($"We have future points with an incoming rate but no control action");
                if (_currentDataService.TimeSeries.Any(x => x.ActualBatteryPercentage.HasValue))
                {
                    _logger.LogInformation($"We have points with actual battery percentage");
                    _controlPlanFactory.CreatePlan();
                    return Task.FromResult(true);
                }
                else
                {
                    _logger.LogWarning($"We do not have points with actual battery percentage");
                }
                return Task.FromResult(false);
            }
            else
            {
                _logger.LogInformation($"No future points exist with an incoming rate but no control action, plan update not required");
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
                _logger.LogInformation($"Got tariff rates for mpan {meter.MPAN}");
                process(agileRates);
                return true;
            }
            _logger.LogWarning($"Did not get tariff rates for mpan {meter.MPAN}");

            return false;
        }

        public async Task<bool> UpdateSolcastData()
        {
            _logger.LogInformation($"Getting solar forecast");
            var forecast = await _dataStore.GetSolarForecast();

            if (forecast != null)
            {
                _logger.LogInformation($"Got solar forecast");
                _currentDataService.Update(forecast);
                return true;
            }

            _logger.LogWarning($"Did not get solar forecast");
            return false;
        }
    }
}
