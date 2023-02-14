using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class AllSettings
    {
        [JsonProperty("data")]
        public List<Setting>? Settings { get; set; }
    }
}
