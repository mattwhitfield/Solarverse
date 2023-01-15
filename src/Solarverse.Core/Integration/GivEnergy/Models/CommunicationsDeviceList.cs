using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class CommunicationsDeviceList
    {
        [JsonProperty("data")]
        public List<CommunicationsDevice>? CommunicationsDevices { get; set; }
    }


}
