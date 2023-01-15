using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class CurrentStateData
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("solar")]
        public Solar? Solar { get; set; }

        [JsonProperty("battery")]
        public Battery? Battery { get; set; }
    }


}
