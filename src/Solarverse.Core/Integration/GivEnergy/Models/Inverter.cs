using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class Inverter
    {
        [JsonProperty("serial")]
        public string? SerialNumber { get; set; }
    }
}
