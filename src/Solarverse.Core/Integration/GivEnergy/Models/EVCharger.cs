using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class EVCharger
    {
        [JsonProperty("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonProperty("type")]
        public string? ChargerType { get; set; }

        [JsonProperty("online")]
        public bool Online { get; set; }

        [JsonProperty("uuid")]
        public string? Uuid { get; set; }
    }
}
