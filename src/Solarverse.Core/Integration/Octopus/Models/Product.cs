using Newtonsoft.Json;

namespace Solarverse.Core.Integration.Octopus.Models
{
    public class Product
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("full_name")] 
        public string? FullName { get; set; }

        [JsonProperty("single_register_electricity_tariffs")]
        public Dictionary<string, TariffType>? TariffTypes { get; set; }
    }
}
