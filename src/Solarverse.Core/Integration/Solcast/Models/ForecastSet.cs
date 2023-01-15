using Newtonsoft.Json;
using Solarverse.Core.Models;

namespace Solarverse.Core.Integration.Solcast.Models
{
    public class ForecastSet
    {
        [JsonProperty("forecasts")]
        public List<Forecast>? Forecasts { get; set; }

        internal SolarForecast ToSolarForecast()
        {
            var normalized = new NormalizedForecast(this);
            return new SolarForecast(
                normalized.IsValid,
                normalized.DataPoints.Select(x => new SolarForecastPoint(x.Time, x.PVEstimate)));
        }
    }
}
