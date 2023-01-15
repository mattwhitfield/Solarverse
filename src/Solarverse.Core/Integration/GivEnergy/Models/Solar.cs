using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class Solar
    {
        [JsonProperty("power")]
        public int Power { get; set; }

        [JsonProperty("arrays")]
        public List<Array>? Arrays { get; set; }
    }
}
