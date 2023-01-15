using Newtonsoft.Json;
using System.Diagnostics;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    [DebuggerDisplay("Time = {Time}, Consumption = {Today?.Consumption}")]
    public class ConsumptionDataPoint
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("today")]
        public Today? Today { get; set; }
    }
}
