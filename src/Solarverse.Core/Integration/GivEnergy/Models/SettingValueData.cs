using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class SettingValueData
    {
        [JsonProperty("data")]
        public SettingValue? Data { get; set; }
    }
}
