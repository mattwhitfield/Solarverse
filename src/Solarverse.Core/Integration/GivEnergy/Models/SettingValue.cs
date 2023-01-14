using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class SettingValue
    {
        [JsonProperty("value")]
        public object? Value { get; set; }
    }

    public class SettingValueData
    {
        [JsonProperty("data")]
        public SettingValue? Data { get; set; }
    }
}
