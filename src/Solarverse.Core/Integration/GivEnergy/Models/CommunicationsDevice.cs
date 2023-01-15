using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class CommunicationsDevice
    {
        [JsonProperty("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonProperty("type")]
        public string? CommunicationsType { get; set; }

        [JsonProperty("inverter")]
        public Inverter? Inverter { get; set; }
    }


}
