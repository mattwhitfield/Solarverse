using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class TodayGrid
    {
        [JsonProperty("import")]
        public double Import { get; set; }

        [JsonProperty("export")]
        public double Export { get; set; }
    }
}
