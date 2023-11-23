using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class EVChargerList
    {
        [JsonProperty("data")]
        public List<EVCharger>? EVChargers { get; set; }
    }
}
