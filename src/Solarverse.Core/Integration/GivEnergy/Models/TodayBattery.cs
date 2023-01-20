using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class TodayBattery
    {
        [JsonProperty("charge")]
        public double Charge { get; set; }

        [JsonProperty("discharge")]
        public double Discharge { get; set; }
    }
}
