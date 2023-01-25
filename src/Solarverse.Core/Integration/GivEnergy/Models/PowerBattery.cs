using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class PowerBattery
    {
        [JsonProperty("percent")]
        public double Percent { get; set; }
    }
}
