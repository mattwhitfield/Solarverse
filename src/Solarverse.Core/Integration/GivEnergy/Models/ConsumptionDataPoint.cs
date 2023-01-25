using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class ConsumptionDataPoint
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("today")]
        public Today? Today { get; set; }

        [JsonProperty("power")]
        public Power? Power { get; set; }
    }
}
