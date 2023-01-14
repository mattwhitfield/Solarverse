using Newtonsoft.Json;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using System;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.GivEnergy
{
    public class GivEnergyClient : IGivEnergyClient
    {
        private readonly HttpClient _httpClient;

        private string? _inverterSerial;

        private List<Setting> _settings = new List<Setting>();

        public GivEnergyClient(string key)
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
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

        public async Task<CurrentState> GetCurrentState()
        {
            var inverterSerial = await FindInverterSerial();

            return await _httpClient.Get<CurrentState>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/system-data/latest");
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
    }
}
