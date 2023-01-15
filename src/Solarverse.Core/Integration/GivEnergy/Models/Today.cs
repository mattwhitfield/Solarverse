using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class Today
    {
        [JsonProperty("consumption")]
        public double Consumption { get; set; }
    }
}
