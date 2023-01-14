using Newtonsoft.Json;

namespace Solarverse.Core.Integration.Octopus.Models
{
    public class MeterPoint
    {
        [JsonProperty("gsp")]
        public string? GridSupplyPoint { get; set; }

        [JsonProperty("mpan")]
        public string? MPAN { get; set; }
    }
}
