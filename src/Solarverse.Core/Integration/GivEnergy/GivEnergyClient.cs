using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Models;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.GivEnergy
{
    public class GivEnergyClient : IInverterClient
    {
        private readonly HttpClient _httpClient;

        private string? _inverterSerial;

        private List<Setting> _settings = new List<Setting>();

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

        public async Task<InverterCurrentState> GetCurrentState()
        {
            var inverterSerial = await FindInverterSerial();

            var currentState = await _httpClient.Get<CurrentState>($"https://api.givenergy.cloud/v1/inverter/{inverterSerial}/system-data/latest");

            if (currentState.Data?.Solar == null || currentState.Data?.Battery == null)
            {
                throw new InvalidDataException("Current state query from GivEnergy API was not complete");
            }

            // TODO - read registers to determine this
            var controlAction = ControlAction.Hold;

            return new InverterCurrentState(currentState.Data.Time, currentState.Data.Solar.Power, currentState.Data.Battery.Percent, controlAction);
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

        public Task<HouseholdConsumption> GetHouseholdConsumptionFor(DateTime date)
        {
            // TODO
            throw new NotImplementedException();
        }

        public Task Charge(DateTime until)
        {
            // TODO
            // set charge from -> datetime.utcnow
            // set charge to -> until
            // set enable charge
            // disable discharge
            throw new NotImplementedException();
        }

        public Task Hold()
        {
            // disable charge
            // disable discharge
            // disable eco
            throw new NotImplementedException();
        }

        public Task Discharge()
        {
            // disable charge
            // disable discharge
            // enable eco
            throw new NotImplementedException();
        }

        public Task Export(DateTime until)
        {
            // set discharge from -> datetime.utcnow
            // set discharge to -> until
            // disable charge
            // enable discharge
            throw new NotImplementedException();
        }
    }
}
