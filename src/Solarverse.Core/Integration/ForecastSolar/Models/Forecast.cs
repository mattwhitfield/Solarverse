using Solarverse.Core.Models;

namespace Solarverse.Core.Integration.ForecastSolar.Models
{
    public class Forecast
    {
        public Dictionary<DateTime, int> Result { get; set; }

        public Message Message { get; set; }

        internal SolarForecast ToSolarForecast()
        {
            var normalized = new NormalizedForecast(this);
            return new SolarForecast(
                normalized.IsValid,
                normalized.DataPoints.Select(x => new SolarForecastPoint(x.Time, x.PVEstimate)));
        }
    }
}
