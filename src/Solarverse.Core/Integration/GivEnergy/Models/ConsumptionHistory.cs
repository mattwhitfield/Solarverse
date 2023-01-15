using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class ConsumptionHistory
    {
        [JsonProperty("data")]
        public List<ConsumptionDataPoint>? DataPoints { get; set; }
    }
}
