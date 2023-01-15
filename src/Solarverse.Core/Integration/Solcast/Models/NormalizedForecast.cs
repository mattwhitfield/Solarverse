using System.Xml;

namespace Solarverse.Core.Integration.Solcast.Models
{
    public class NormalizedForecast
    {
        public NormalizedForecast(ForecastSet? forecast)
        {
            if (forecast?.Forecasts == null || !forecast.Forecasts.Any())
            {
                IsValid = false;
                return;
            }

            var allPoints = new List<NormalizedForecastPoint>();

            foreach (var item in forecast.Forecasts?.OrderBy(x => x.PeriodEnd) ?? Enumerable.Empty<Forecast>())
            {
                if (item.PeriodType != null)
                {
                    allPoints.Add(new NormalizedForecastPoint(item.PeriodEnd.Subtract(XmlConvert.ToTimeSpan(item.PeriodType)), item.PVEstimate));
                }
            }

            var date = allPoints.Min(x => x.Time).Date;

            foreach (var dataPoint in allPoints.GroupBy(x => (int)((x.Time - date).TotalMinutes / 30)))
            {
                // we average out all the points in the group, then divide by 2 - because estimate is in kW, and we want kWh for the 1/2 hour period
                var production = dataPoint.Average(x => x.PVEstimate) / 2;
                DataPoints.Add(new NormalizedForecastPoint(date.AddMinutes(dataPoint.Key * 30), production));
            }
        }

        public bool IsValid { get; } = true;

        public List<NormalizedForecastPoint> DataPoints { get; } = new List<NormalizedForecastPoint>();
    }

}
