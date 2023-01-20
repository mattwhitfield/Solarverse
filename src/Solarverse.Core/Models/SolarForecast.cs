using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("IsValid = {IsValid}, DataPoints: {DataPoints.Count}")]
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
