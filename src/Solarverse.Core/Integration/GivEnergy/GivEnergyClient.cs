using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Models;
using Solarverse.Core.Models.Settings;
using System.Globalization;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.GivEnergy
{
    public class GivEnergyClient : IInverterClient
    {
        private readonly HttpClient _httpClient;

        private string? _inverterSerial;
        private CurrentSettingValues? _currentSettings;

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
            while (true)
            {
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

                throw new InvalidOperationException($"Could not read setting {id} as a boolean");
            }
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
                        return new TimeSetting(id, time);
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

        public async Task<object?> ReadSetting(int id)
        {
            var inverterSerial = await FindInverterSerial();

            var setting = await _httpClient.Post<SettingValueData>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/settings/{id}/read");

            return setting.Data?.Value;
        }

        public async Task<SettingMutationValues?> SetSetting(int id, object value)
        {
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
                normalized.DataPoints.Select(x => new HouseholdConsumptionDataPoint(x.Time, x.Consumption, x.Solar, x.Import, x.Export, x.Charge, x.Discharge, x.BatteryPercentage)));
        }

        public async Task SetSettingIfRequired(int settingId, Func<CurrentSettingValues, bool> shouldSet, object value)
        {
            if (_currentSettings == null || shouldSet(_currentSettings))
            {
                // TODO - enable when we're happy
                //await SetSetting(settingId, value);
            }
        }

        public async Task Charge(DateTime until)
        {
            // set start time if it's currently later than now or not set
            await SetSettingIfRequired(
                SettingIds.Charge.StartTime,
                x => !x.ChargeSettings.StartTime.Value.HasValue || x.ChargeSettings.StartTime.Value.Value > DateTime.UtcNow.TimeOfDay,
                DateTime.UtcNow.ToString("HH:mm"));

            // set until if it's not what we want or not set
            await SetSettingIfRequired(
                SettingIds.Charge.EndTime,
                x => !x.ChargeSettings.EndTime.Value.HasValue || x.ChargeSettings.EndTime.Value.Value != until.TimeOfDay,
                until.ToString("HH:mm"));

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

            // enable eco if it's enabled
            await SetSettingIfRequired(
                SettingIds.EcoMode,
                x => !x.EcoModeEnabled.Value,
                true);
        }

        public async Task Export(DateTime until)
        {
            // set start time if it's currently later than now or not set
            await SetSettingIfRequired(
                SettingIds.Discharge.StartTime,
                x => !x.DischargeSettings.StartTime.Value.HasValue || x.DischargeSettings.StartTime.Value.Value > DateTime.UtcNow.TimeOfDay,
                DateTime.UtcNow.ToString("HH:mm"));

            // set until if it's not what we want or not set
            await SetSettingIfRequired(
                SettingIds.Discharge.EndTime,
                x => !x.DischargeSettings.EndTime.Value.HasValue || x.DischargeSettings.EndTime.Value.Value != until.TimeOfDay,
                until.ToString("HH:mm"));

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
    }
}
