namespace Solarverse.Core.Integration.ForecastSolar.Models
{
    public class NormalizedForecast
    {
        public NormalizedForecast(Forecast? forecast)
        {
            if (forecast?.Result == null || !forecast.Result.Any() || !string.Equals(forecast.Message.Type, "success", StringComparison.OrdinalIgnoreCase))
            {
                IsValid = false;
                return;
            }

            var allPoints = new List<NormalizedForecastPoint>();

            foreach (var item in forecast.Result.OrderBy(x => x.Key))
            {
                allPoints.Add(new NormalizedForecastPoint(item.Key, item.Value));
            }

            var date = allPoints.Min(x => x.Time).Date;

            foreach (var dataPoint in allPoints.GroupBy(x => (int)((x.Time - date).TotalMinutes / 30)))
            {
                // we average out all the points in the group, then divide by 2000 - because estimate is in watts, and we want kWh for the 1/2 hour period
                var production = dataPoint.Average(x => x.PVEstimate) / 2000;
                DataPoints.Add(new NormalizedForecastPoint(date.AddMinutes(dataPoint.Key * 30), production));
            }
        }

        public bool IsValid { get; } = true;

        public List<NormalizedForecastPoint> DataPoints { get; } = new List<NormalizedForecastPoint>();
    }

}
