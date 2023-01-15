using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class Battery
    {
        [JsonProperty("percent")]
        public int Percent { get; set; }

        [JsonProperty("power")]
        public int Power { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }


}
