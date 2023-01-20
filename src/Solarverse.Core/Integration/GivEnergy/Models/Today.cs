using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class Today
    {
        [JsonProperty("consumption")]
        public double Consumption { get; set; }

        [JsonProperty("solar")]
        public double Solar { get; set; }

        [JsonProperty("battery")]
        public TodayBattery? Battery { get; set; }

        [JsonProperty("grid")]
        public TodayGrid? Grid { get; set; }
    }
}
