using Newtonsoft.Json;

namespace Solarverse.Core.Integration.Octopus.Models
{
    public class Tariff
    {
        [JsonProperty("code")]
        public string? Code { get; set; }
    }
}
