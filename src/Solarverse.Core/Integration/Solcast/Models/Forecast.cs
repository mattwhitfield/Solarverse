using Newtonsoft.Json;

namespace Solarverse.Core.Integration.Solcast.Models
{
    public class Forecast
    {
        [JsonProperty("pv_estimate")]
        public double PVEstimate { get; set; }

        [JsonProperty("pv_estimate10")]
        public double PVEstimate10thPercentile { get; set; }

        [JsonProperty("pv_estimate90")]
        public double PVEstimate90thPercentile { get; set; }

        [JsonProperty("period_end")]
        public DateTime PeriodEnd { get; set; }

        [JsonProperty("period")]
        public string PeriodType { get; set; }
    }

}
