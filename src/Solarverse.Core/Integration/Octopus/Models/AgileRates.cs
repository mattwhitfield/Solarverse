using Newtonsoft.Json;

namespace Solarverse.Core.Integration.Octopus.Models
{
    public class AgileRates
    {
        [JsonProperty("results")]
        public List<AgileRate>? Rates { get; set; }
    }
}
