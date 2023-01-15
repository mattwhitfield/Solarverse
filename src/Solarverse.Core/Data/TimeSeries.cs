using Solarverse.Core.Helper;

namespace Solarverse.Core.Data
{
    public class TimeSeries
    {
        private Dictionary<DateTime, TimeSeriesPoint> _dataPoints = new Dictionary<DateTime, TimeSeriesPoint>();

        public void AddPointsFrom<T>(IEnumerable<T> points, Func<T, DateTime> dateTime, Func<T, double?> value, Action<double?, TimeSeriesPoint> set)
        {
            foreach (var point in points)
            {
                var pointTime = dateTime(point);
                if (!_dataPoints.TryGetValue(pointTime, out var dataPoint))
                {
                    _dataPoints[pointTime] = dataPoint = new TimeSeriesPoint();
                }
                set(value(point), dataPoint);
            }
        }

        public IList<RenderedTimeSeriesPoint> GetSeries(Func<TimeSeriesPoint, double?> value)
        {
            return _dataPoints.OrderBy(point => point.Key).Select(point => new RenderedTimeSeriesPoint(point.Key, value(point.Value))).ToList();
        }

        internal bool Cull(TimeSpan deleteOlderThan)
        {
            var olderPoints = _dataPoints.Keys.Where(x => x < DateTime.UtcNow.Subtract(deleteOlderThan)).ToList();
            olderPoints.Each(key => _dataPoints.Remove(key));
            return olderPoints.Any();
        }

        internal bool TryGetDataPointFor(DateTime currentPeriod, out TimeSeriesPoint? currentDataPoint)
        {
            if (_dataPoints.TryGetValue(currentPeriod, out var current))
            {
                currentDataPoint = current;
                return true;
            }

            currentDataPoint = default;
            return false;
        }
    }
}
