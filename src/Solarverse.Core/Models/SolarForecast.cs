namespace Solarverse.Core.Models
{
    public class SolarForecast
    {
        public SolarForecast(bool isValid, IEnumerable<SolarForecastPoint> dataPoints)
        {
            IsValid = isValid;
            DataPoints = dataPoints.ToList();
        }

        public bool IsValid { get; }

        public IList<SolarForecastPoint> DataPoints { get; }
    }

}
