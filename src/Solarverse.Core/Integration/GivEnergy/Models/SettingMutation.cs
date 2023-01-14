using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class SettingMutationValues
    {
        [JsonProperty("value")]
        public object? Value { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public class SettingMutation
    {
        [JsonProperty("data")]
        public SettingMutationValues? Data { get; set; }
    }
}
