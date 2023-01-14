using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    internal class AllSettings
    {
        [JsonProperty("data")]
        public List<Setting>? Settings { get; set; }
    }

    public class Setting
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("validation")]
        public string? Validation { get; set; }
    }
}
