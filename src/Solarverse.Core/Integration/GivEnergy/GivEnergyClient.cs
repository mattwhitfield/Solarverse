using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Models;
using System.Globalization;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.GivEnergy
{
    public class GivEnergyClient : IInverterClient
    {
        private readonly HttpClient _httpClient;

        private string? _inverterSerial;
        private CurrentSettingValues? _currentSettings;
        private readonly List<Setting> _settings = new();

        public GivEnergyClient(Configuration configuration)
        {
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(configuration.ApiKeys?.GivEnergy))
            {
                throw new InvalidOperationException("GivEnergy API key was not configured");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.ApiKeys.GivEnergy);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> FindInverterSerial()
        {
            if (string.IsNullOrWhiteSpace(_inverterSerial))
            {
                var inverterData = await _httpClient.Get<CommunicationsDeviceList>("https://api.givenergy.cloud/v1/communication-device");
                if (inverterData == null || inverterData.CommunicationsDevices == null || !inverterData.CommunicationsDevices.Any(x => x.Inverter != null))
                {
                    throw new InvalidOperationException("Could not get communication device list from GivEnergy API");
                }

                var commsDevice = inverterData.CommunicationsDevices.First(x => x.Inverter != null);

                _inverterSerial = commsDevice.Inverter?.SerialNumber ?? string.Empty;
            }

            return _inverterSerial;
        }

        private async Task<BoolSetting> GetBoolSetting(int id)
        {
            var settingValue = await ReadSetting(id);
            if (settingValue is bool b)
            {
                return new BoolSetting(id, b);
            }

            throw new InvalidOperationException($"Could not read setting {id} as a boolean");
        }

        private async Task<IntSetting> GetIntSetting(int id)
        {
            var settingValue = await ReadSetting(id);
            if (settingValue is int i)
            {
                return new IntSetting(id, i);
            }
            if (settingValue is long l)
            {
                return new IntSetting(id, (int)l);
            }

            throw new InvalidOperationException($"Could not read setting {id} as an integer");
        }

        private async Task<TimeSetting> GetTimeSetting(int id)
        {
            var settingValue = await ReadSetting(id);
            if (settingValue is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return new TimeSetting(id, null);
                }

                if (TimeSpan.TryParseExact(s, "hh\\:mm", CultureInfo.InvariantCulture, out var time))
                {
                    return new TimeSetting(id, time);
                }
            }

            if (settingValue is null)
            {
                return new TimeSetting(id, null);
            }

            throw new InvalidOperationException($"Could not read setting {id} as a TimeSpan");
        }

        public async Task<InverterCurrentState> GetCurrentState()
        {
            var inverterSerial = await FindInverterSerial();

            var currentState = await _httpClient.Get<CurrentState>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/system-data/latest");

            if (currentState.Data?.Solar == null || currentState.Data?.Battery == null)
            {
                throw new InvalidDataException("Current state query from GivEnergy API was not complete");
            }

            var ecoModeEnabled = await GetBoolSetting(SettingIds.EcoMode);
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

            var maxDischargeRateKw = dischargeSettings.PowerLimit.Value / 1000.0;
            var maxChargeRateKw = chargeSettings.PowerLimit.Value / 1000.0;

            return new InverterCurrentState(
                currentState.Data.Time,
                currentState.Data.Solar.Power,
                currentState.Data.Battery.Percent,
                maxDischargeRateKw,
                maxChargeRateKw);
        }

        private async Task<BatteryModeSettingValues> GetBatterySettings(int startTimeSettingId, int endTimeSettingId, int enabledSettingId, int powerLimitSettingId)
        {
            var startTimeTask = GetTimeSetting(startTimeSettingId);
            var endTimeTask = GetTimeSetting(endTimeSettingId);
            var enabledTask = GetBoolSetting(enabledSettingId);
            var powerLimitTask = GetIntSetting(powerLimitSettingId);
            await Task.WhenAll(startTimeTask, endTimeTask, enabledTask, powerLimitTask);

            return new BatteryModeSettingValues(
                startTimeTask.Result,
                endTimeTask.Result,
                enabledTask.Result,
                powerLimitTask.Result);
        }

        public async Task<IEnumerable<Setting>> GetAllSettings()
        {
            if (!_settings.Any())
            {
                var inverterSerial = await FindInverterSerial();

                var allSettings = await _httpClient.Get<AllSettings>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/settings");
                if (allSettings?.Settings == null)
                {
                    throw new InvalidOperationException("Could not read inverter settings");
                }

                _settings.AddRange(allSettings.Settings);
            }

            return _settings;
        }

        public async Task<int> GetSettingId(string settingName)
        {
            var allSettings = await GetAllSettings();

            var setting = allSettings.FirstOrDefault(x => string.Equals(x.Name, settingName, StringComparison.OrdinalIgnoreCase));
            if (setting == null)
            {
                throw new InvalidOperationException($"Could not find the setting with the name '{settingName}'");
            }

            return setting.Id;
        }

        public async Task<object?> ReadSetting(int id)
        {
            var allSettings = await GetAllSettings();
            if (!allSettings.Any(x => x.Id == id))
            {
                throw new InvalidOperationException($"Could not find the setting with the ID {id}");
            }

            var inverterSerial = await FindInverterSerial();

            var setting = await _httpClient.Post<SettingValueData>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/settings/{id}/read");

            return setting.Data?.Value;
        }

        public async Task<SettingMutationValues?> SetSetting(int id, object value)
        {
            var allSettings = await GetAllSettings();
            if (!allSettings.Any(x => x.Id == id))
            {
                throw new InvalidOperationException($"Could not find the setting with the ID {id}");
            }

            var inverterSerial = await FindInverterSerial();

            int attempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);

            SettingMutation? setting = null;
            while (attempts < 10)
            {
                setting = await _httpClient.Post<SettingMutation>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/settings/{id}/write", new SettingValue { Value = value });

                if (setting.Data != null && setting.Data.Success)
                {
                    return setting.Data;
                }

                attempts++;
                await Task.Delay(delayTime);
                delayTime *= 1.5;
            }

            return setting?.Data;
        }

        public async Task<HouseholdConsumption> GetHouseholdConsumptionFor(DateTime date)
        {
            var inverterSerial = await FindInverterSerial();

            var history = await _httpClient.Get<ConsumptionHistory>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/data-points/{date.Year}-{date.Month}-{date.Day}?pageSize=288");

            var normalized = new NormalizedConsumption(history);

            return new HouseholdConsumption(
                normalized.IsValid,
                normalized.ContainsInterpolatedPoints,
                normalized.DataPoints.Select(x => new HouseholdConsumptionDataPoint(x.Time, x.Consumption)));
        }

        public Task Charge(DateTime until)
        {
            // TODO
            // if charge from is later than DateTime.UtcNow
            //   set charge from -> datetime.utcnow
            // set charge to -> until
            // set enable charge
            // disable discharge
            throw new NotImplementedException();
        }

        public Task Hold(DateTime until)
        {
            // TODO
            // disable charge
            // disable discharge
            // disable eco
            throw new NotImplementedException();
        }

        public Task Discharge(DateTime until)
        {
            // TODO
            // disable charge
            // disable discharge
            // enable eco
            throw new NotImplementedException();
        }

        public Task Export(DateTime until)
        {
            // TODO
            // if discharge from is later than DateTime.UtcNow
            //   set discharge from -> datetime.utcnow
            // set discharge to -> until
            // disable charge
            // enable discharge
            throw new NotImplementedException();
        }
    }
}
