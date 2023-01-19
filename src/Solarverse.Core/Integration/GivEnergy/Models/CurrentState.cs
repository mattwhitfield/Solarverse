using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class CurrentState
    {
        [JsonProperty("data")]
        public CurrentStateData? Data { get; set; }
    }
}
