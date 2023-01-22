using Solarverse.Core.Helper;
using System.Diagnostics.CodeAnalysis;

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

        public IList<DateTime> GetDates()
        {
            return _dataPoints.Select(x => x.Key.Date).Distinct().ToList();
        }

        public bool Cull(TimeSpan deleteOlderThan)
        {
            var olderPoints = _dataPoints.Keys.Where(x => x < DateTime.UtcNow.Subtract(deleteOlderThan)).ToList();
            olderPoints.Each(key => _dataPoints.Remove(key));
            return olderPoints.Any();
        }

        public bool TryGetDataPointFor(DateTime currentPeriod, [NotNullWhen(returnValue: true)] out TimeSeriesPoint? currentDataPoint)
        {
            if (_dataPoints.TryGetValue(currentPeriod, out var current))
            {
                currentDataPoint = current;
                return true;
            }

            currentDataPoint = default;
            return false;
        }

        public DateTime? GetMaximumDate() => GetMaximumDate(_ => true);

        public DateTime? GetMaximumDate(Func<TimeSeriesPoint, bool> predicate)
        {
            var eligible = _dataPoints.Where(x => predicate(x.Value)).Select(x => x.Key);
            if (!eligible.Any())
            {
                return null;
            }

            return eligible.Max();
        }

        public DateTime GetMinimumDate()
        {
            if (!_dataPoints.Any())
            {
                return DateTime.MinValue;
            }

            return _dataPoints.Keys.Min();
        }

        internal void Set(Action<TimeSeriesPoint> action)
        {
            _dataPoints.Values.Each(action);
        }
    }
}
