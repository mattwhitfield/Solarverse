using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Models;
using System.Globalization;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.GivEnergy
{
    public class GivEnergyClient : IInverterClient, IEVChargerClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GivEnergyClient> _logger;
        private readonly ICurrentDataService _currentDataService;
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private string? _inverterSerial;
        private string? _evChargerUuid;
        private CurrentSettingValues? _currentSettings;

        public bool UsesLocalTimeBoundary => true;

        public GivEnergyClient(ILogger<GivEnergyClient> logger, IConfigurationProvider configurationProvider, ICurrentDataService currentDataService, ICurrentTimeProvider currentTimeProvider)
        {
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(configurationProvider.Configuration.ApiKeys?.GivEnergy))
            {
                throw new InvalidOperationException("GivEnergy API key was not configured");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configurationProvider.Configuration.ApiKeys.GivEnergy);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
            _currentDataService = currentDataService;
            _currentTimeProvider = currentTimeProvider;
        }

        public async Task<string> FindInverterSerial()
        {
            if (string.IsNullOrWhiteSpace(_inverterSerial))
            {
                var inverterData = await _httpClient.Get<CommunicationsDeviceList>(_logger, "https://api.givenergy.cloud/v1/communication-device");
                if (inverterData == null || inverterData.CommunicationsDevices == null || !inverterData.CommunicationsDevices.Any(x => x.Inverter != null))
                {
                    throw new InvalidOperationException("Could not get communication device list from GivEnergy API");
                }

                var commsDevice = inverterData.CommunicationsDevices.First(x => x.Inverter != null);

                _inverterSerial = commsDevice.Inverter?.SerialNumber ?? string.Empty;
                _logger.LogInformation($"Inverter serial # is {_inverterSerial}");
            }

            return _inverterSerial;
        }

        public async Task<string> FindEVChargerUuid()
        {
            if (string.IsNullOrWhiteSpace(_evChargerUuid))
            {
                var evChargers = await _httpClient.Get<EVChargerList>(_logger, "https://api.givenergy.cloud/v1/ev-charger/");
                if (evChargers == null || evChargers.EVChargers == null || !evChargers.EVChargers.Any(x => x.Online))
                {
                    throw new InvalidOperationException("Could not get EV Charger list from GivEnergy API");
                }

                _evChargerUuid = evChargers.EVChargers.Where(x => x.Online).Select(x => x.Uuid).FirstOrDefault() ?? string.Empty;
                _logger.LogInformation($"EV Charger UUID is {_evChargerUuid}");
            }

            return _evChargerUuid;
        }

        private async Task<BoolSetting> GetBoolSetting(int id)
        {
            while (true)
            {
                _logger.LogInformation($"Reading setting {id} ({SettingIds.GetName(id)}) as boolean");

                var settingValue = await ReadSetting(id);
                if (settingValue is bool b)
                {
                    return new BoolSetting(id, b);
                }

                if (settingValue is int i && i == -1)
                {
                    await Task.Delay(500);
                    continue;
                }

                throw new InvalidOperationException($"Could not read setting {id} as a boolean - got {settingValue ?? "<null>"}");
            }
        }

        private async Task<IntSetting> GetIntSetting(int id)
        {
            _logger.LogInformation($"Reading setting {id} ({SettingIds.GetName(id)}) as integer");

            var settingValue = await ReadSetting(id);
            if (settingValue is int i)
            {
                return new IntSetting(id, i);
            }
            if (settingValue is long l)
            {
                return new IntSetting(id, (int)l);
            }

            throw new InvalidOperationException($"Could not read setting {id} as an integer - got {settingValue ?? "<null>"}");
        }

        private async Task<TimeSetting> GetTimeSetting(int id)
        {
            _logger.LogInformation($"Reading setting {id} ({SettingIds.GetName(id)}) as time");

            while (true)
            {
                var settingValue = await ReadSetting(id);
                if (settingValue is null)
                {
                    return new TimeSetting(id, null);
                }

                if (settingValue is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return new TimeSetting(id, null);
                    }

                    if (TimeSpan.TryParseExact(s, "hh\\:mm", CultureInfo.InvariantCulture, out var time))
                    {
                        var offset = _currentTimeProvider.Offset;
                        var result = time - offset;
                        if (result < TimeSpan.Zero)
                        {
                            result += TimeSpan.FromDays(1);
                        }

                        _logger.LogInformation($"Time setting - raw {s}, parsed {result}");

                        return new TimeSetting(id, result);
                    }
                }

                if (settingValue is int i && i == -1)
                {
                    await Task.Delay(500);
                    continue;
                }

                return new TimeSetting(id, null);
            }
        }

        public async Task<InverterCurrentState?> GetCurrentState()
        {
            var inverterSerial = await FindInverterSerial();

            var currentState = await _httpClient.Get<CurrentState>(_logger, $"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/system-data/latest");

            if (currentState.Data?.Solar == null || currentState.Data?.Battery == null)
            {
                throw new InvalidDataException("Current state query from GivEnergy API was not complete");
            }

            var ecoModeEnabled = await GetBoolSetting(SettingIds.EcoMode);
            var batteryReserve = await GetIntSetting(SettingIds.Discharge.Reserve);
            var dischargeSettings = await GetBatterySettings(
                SettingIds.Discharge.StartTime,
                SettingIds.Discharge.EndTime,
                SettingIds.Discharge.Enabled,
                SettingIds.Discharge.PowerLimit);
            var chargeSettings = await GetBatterySettings(
                SettingIds.Charge.StartTime,
                SettingIds.Charge.EndTime,
                SettingIds.Charge.Enabled,
                SettingIds.Charge.PowerLimit);

            _currentSettings = new CurrentSettingValues(
                ecoModeEnabled,
                chargeSettings,
                dischargeSettings);

            if (dischargeSettings.PowerLimit.Value < 0 || chargeSettings.PowerLimit.Value < 0 || batteryReserve.Value < 0)
            {
                return null;
            }

            var maxDischargeRateKw = Math.Max(dischargeSettings.PowerLimit.Value / 1000.0, 0);
            var maxChargeRateKw = Math.Max(chargeSettings.PowerLimit.Value / 1000.0, 0);

            var state = new InverterCurrentState(
                currentState.Data.Time,
                currentState.Data.Solar.Power,
                currentState.Data.Battery.Percent,
                maxDischargeRateKw,
                maxChargeRateKw,
                batteryReserve.Value);

            state.ExtendedProperties[SettingIds.GetName(SettingIds.EcoMode)] = ecoModeEnabled.Value.ToString();
            state.ExtendedProperties[SettingIds.GetName(SettingIds.Charge.Enabled)] = chargeSettings.Enabled.Value.ToString();
            state.ExtendedProperties[SettingIds.GetName(SettingIds.Charge.StartTime)] = chargeSettings.StartTime.Value?.ToString() ?? "<not set>";
            state.ExtendedProperties[SettingIds.GetName(SettingIds.Charge.EndTime)] = chargeSettings.EndTime.Value?.ToString() ?? "<not set>";
            state.ExtendedProperties[SettingIds.GetName(SettingIds.Discharge.Enabled)] = dischargeSettings.Enabled.Value.ToString();
            state.ExtendedProperties[SettingIds.GetName(SettingIds.Discharge.StartTime)] = dischargeSettings.StartTime.Value?.ToString() ?? "<not set>";
            state.ExtendedProperties[SettingIds.GetName(SettingIds.Discharge.EndTime)] = dischargeSettings.EndTime.Value?.ToString() ?? "<not set>";

            _logger.LogInformation($"Inverter state updated @ {state.UpdateTime}:");
            _logger.LogInformation($"  Battery {state.BatteryPercent}%");
            _logger.LogInformation($"  Battery reserve {state.BatteryReserve}%");
            _logger.LogInformation($"  Current solar power {state.CurrentSolarPower}");
            _logger.LogInformation($"  Max charge rate Kw {state.MaxChargeRateKw}");
            _logger.LogInformation($"  Max discharge rate Kw {state.MaxDischargeRateKw}");
            _logger.LogInformation($"Internal inverter state:");
            _logger.LogInformation($"  Eco - Enabled: {ecoModeEnabled.Value}");
            _logger.LogInformation($"  Charge - Enabled: {chargeSettings.Enabled.Value}, From: {chargeSettings.StartTime.Value}, To {chargeSettings.EndTime.Value}");
            _logger.LogInformation($"  Discharge - Enabled: {dischargeSettings.Enabled.Value}, From: {dischargeSettings.StartTime.Value}, To {dischargeSettings.EndTime.Value}");

            return state;
        }

        private async Task<BatteryModeSettingValues> GetBatterySettings(int startTimeSettingId, int endTimeSettingId, int enabledSettingId, int powerLimitSettingId)
        {
            var startTime = await GetTimeSetting(startTimeSettingId);
            var endTime = await GetTimeSetting(endTimeSettingId);
            var enabled = await GetBoolSetting(enabledSettingId);
            var powerLimit = await GetIntSetting(powerLimitSettingId);

            return new BatteryModeSettingValues(
                startTime,
                endTime,
                enabled,
                powerLimit);
        }

        public async Task<object?> ReadSetting(int id)
        {
            var inverterSerial = await FindInverterSerial();

            var setting = await _httpClient.Post<SettingValueData>(_logger, $"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/settings/{id}/read");

            return setting.Data?.Value;
        }

        public async Task<SettingMutationValues?> SetSetting(int id, object value)
        {
            var inverterSerial = await FindInverterSerial();

            int attempts = 0;
            int inverterTimeoutAttempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);

            SettingMutation? setting = null;
            while (attempts < 10 && inverterTimeoutAttempts < 50)
            {
                setting = await _httpClient.Post<SettingMutation>(_logger, $"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/settings/{id}/write", new SettingValue { Value = value });

                if (setting.Data != null && setting.Data.Success)
                {
                    _currentDataService.UpdateCurrentState(x =>
                        x.ExtendedProperties[SettingIds.GetName(id)] = value?.ToString() ?? "<null>"
                    );
                    return setting.Data;
                }

                _logger.LogWarning($"Could not set setting {id} to value {value} - error was {setting?.Data?.Message}");

                if (!string.Equals(setting?.Data?.Message, "Inverter Timeout", StringComparison.OrdinalIgnoreCase))
                {
                    attempts++;
                }
                else
                {
                    inverterTimeoutAttempts++;
                }

                await Task.Delay(delayTime);
                _logger.LogWarning($"Will delay {delayTime} until next retry");
                delayTime *= 1.5;
            }

            return setting?.Data;
        }

        public async Task SetEVChargerState(bool charge)
        {
            var evChargerUuid = await FindEVChargerUuid();

            int attempts = 0;
            int inverterTimeoutAttempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);

            var command = charge ? "start-charge" : "stop-charge";

            SettingMutation? setting = null;
            while (attempts < 10 && inverterTimeoutAttempts < 50)
            {
                setting = await _httpClient.Post<SettingMutation>(_logger, $"https://api.givenergy.cloud/v1/ev-charger/{evChargerUuid}/commands/{command}");

                if (setting.Data != null && setting.Data.Success)
                {
                    return;
                }

                _logger.LogWarning($"Could not send command {command} to charge {evChargerUuid} - error was {setting?.Data?.Message}");

                if (!string.Equals(setting?.Data?.Message, "Inverter Timeout", StringComparison.OrdinalIgnoreCase))
                {
                    attempts++;
                }
                else
                {
                    inverterTimeoutAttempts++;
                }

                await Task.Delay(delayTime);
                _logger.LogWarning($"Will delay {delayTime} until next retry");
                delayTime *= 1.5;
            }
        }

        public async Task<HouseholdConsumption> GetHouseholdConsumptionFor(DateTime date)
        {
            var inverterSerial = await FindInverterSerial();

            var history = await _httpClient.Get<ConsumptionHistory>(_logger, $"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/data-points/{date.Year}-{date.Month}-{date.Day}?pageSize=1000");

            var normalized = new NormalizedConsumption(history, _currentTimeProvider);

            return new HouseholdConsumption(
                normalized.IsValid,
                normalized.ContainsInterpolatedPoints,
                normalized.DataPoints.Select(x => new HouseholdConsumptionDataPoint(x.Time, x.Consumption, x.Solar, x.Import, x.Export, x.Charge, x.Discharge, x.BatteryPercentage)));
        }

        public async Task SetSettingIfRequired(int settingId, Func<CurrentSettingValues, bool> shouldSet, object value)
        {
            _logger.LogInformation($"Setting {settingId} ({SettingIds.GetName(settingId)}) to value {value}");
            if (_currentSettings == null || shouldSet(_currentSettings))
            {
                _logger.LogInformation($"Setting {settingId} ({SettingIds.GetName(settingId)}) needs to be set");
                await SetSetting(settingId, value);
            }
            else
            {
                _logger.LogInformation($"Setting {settingId} ({SettingIds.GetName(settingId)}) is already set to the required value");
            }
        }

        public async Task Charge(DateTime until)
        {
            _logger.LogInformation($"Enabling Charge mode until {until}");

            // set start time if it's currently later than now or not set
            await SetSettingIfRequired(
                SettingIds.Charge.StartTime,
                x => !x.ChargeSettings.StartTime.Value.HasValue || _currentTimeProvider.ToLocalTime(x.ChargeSettings.StartTime.Value.Value) > _currentTimeProvider.LocalNow.TimeOfDay,
                _currentTimeProvider.LocalNow.ToString("HH:mm"));

            // set until if it's not what we want or not set
            var untilLocal = _currentTimeProvider.ToLocalTime(until);
            await SetSettingIfRequired(
                SettingIds.Charge.EndTime,
                x => !x.ChargeSettings.EndTime.Value.HasValue || _currentTimeProvider.ToLocalTime(x.ChargeSettings.EndTime.Value.Value) != untilLocal.TimeOfDay,
                untilLocal.ToString("HH:mm"));

            // enable charge if it's not enabled
            await SetSettingIfRequired(
                SettingIds.Charge.Enabled,
                x => !x.ChargeSettings.Enabled.Value,
                true);

            // disable discharge if it's enabled
            await SetSettingIfRequired(
                SettingIds.Discharge.Enabled,
                x => x.DischargeSettings.Enabled.Value,
                false);
        }

        public async Task Hold(DateTime until)
        {
            _logger.LogInformation($"Enabling Hold mode until {until}");

            // disable charge if it's enabled
            await SetSettingIfRequired(
                SettingIds.Charge.Enabled,
                x => x.ChargeSettings.Enabled.Value,
                false);

            // disable discharge if it's enabled
            await SetSettingIfRequired(
                SettingIds.Discharge.Enabled,
                x => x.DischargeSettings.Enabled.Value,
                false);

            // disable eco if it's enabled
            await SetSettingIfRequired(
                SettingIds.EcoMode,
                x => x.EcoModeEnabled.Value,
                false);
        }

        public async Task Discharge(DateTime until)
        {
            _logger.LogInformation($"Enabling Discharge mode until {until}");

            // disable charge if it's enabled
            await SetSettingIfRequired(
                SettingIds.Charge.Enabled,
                x => x.ChargeSettings.Enabled.Value,
                false);

            // disable discharge if it's enabled
            await SetSettingIfRequired(
                SettingIds.Discharge.Enabled,
                x => x.DischargeSettings.Enabled.Value,
                false);

            // enable eco if it's disabled
            await SetSettingIfRequired(
                SettingIds.EcoMode,
                x => !x.EcoModeEnabled.Value,
                true);
        }

        public async Task Export(DateTime until)
        {
            _logger.LogInformation($"Enabling Export mode until {until}");

            // set start time if it's currently later than now or not set
            await SetSettingIfRequired(
                SettingIds.Discharge.StartTime,
                x => !x.DischargeSettings.StartTime.Value.HasValue || x.DischargeSettings.StartTime.Value.Value > _currentTimeProvider.LocalNow.TimeOfDay,
                _currentTimeProvider.LocalNow.ToString("HH:mm"));

            // set until if it's not what we want or not set
            var untilLocal = _currentTimeProvider.ToLocalTime(until);
            await SetSettingIfRequired(
                SettingIds.Discharge.EndTime,
                x => !x.DischargeSettings.EndTime.Value.HasValue || x.DischargeSettings.EndTime.Value.Value != untilLocal.TimeOfDay,
                untilLocal.ToString("HH:mm"));

            // enable charge if it's not enabled
            await SetSettingIfRequired(
                SettingIds.Discharge.Enabled,
                x => !x.DischargeSettings.Enabled.Value,
                true);

            // disable charge if it's enabled
            await SetSettingIfRequired(
                SettingIds.Charge.Enabled,
                x => x.ChargeSettings.Enabled.Value,
                false);
        }

        public Task SetChargingEnabled(bool enabled)
        {
            _logger.LogInformation($"Setting EV Charge enabled to {enabled}");
            _logger.LogInformation($"EV Charge temporarily disabled");
            return Task.CompletedTask;
            //return SetEVChargerState(enabled);
        }
    }
}
