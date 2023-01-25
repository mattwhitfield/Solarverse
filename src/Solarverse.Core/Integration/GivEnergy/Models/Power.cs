using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class Power
    {
        [JsonProperty("battery")]
        public PowerBattery? Battery { get; set; }
    }
}
