using Solarverse.Core.Helper;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Solarverse.Core.Data
{
    public class TimeSeries : IEnumerable<TimeSeriesPoint>
    {
        public TimeSeries()
        { }

        public TimeSeries(IEnumerable<TimeSeriesPoint> data)
        {
            data.Each(x => _dataPoints[x.Time] = x);
        }

        private Dictionary<DateTime, TimeSeriesPoint> _dataPoints = new Dictionary<DateTime, TimeSeriesPoint>();

        public void AddPointsFrom<TSource, TValue>(IEnumerable<TSource> points, Func<TSource, DateTime> dateTime, Func<TSource, TValue?> value, Action<TValue?, TimeSeriesPoint> set)
        {
            foreach (var point in points)
            {
                var pointTime = dateTime(point);
                if (!_dataPoints.TryGetValue(pointTime, out var dataPoint))
                {
                    _dataPoints[pointTime] = dataPoint = new TimeSeriesPoint(pointTime);
                }
                set(value(point), dataPoint);
            }
        }

        public IList<RenderedNullableTimeSeriesPoint> GetNullableSeries(Func<TimeSeriesPoint, double?> value)
        {
            return _dataPoints.OrderBy(point => point.Key).Select(point => new RenderedNullableTimeSeriesPoint(point.Key, value(point.Value))).ToList();
        }

        public IList<RenderedTimeSeriesPoint> GetSeries(Func<TimeSeriesPoint, double?> value)
        {
            var output = new List<RenderedTimeSeriesPoint>();

            foreach (var point in _dataPoints.OrderBy(x => x.Key))
            {
                var pointValue = value(point.Value);
                if (pointValue.HasValue)
                {
                    output.Add(new RenderedTimeSeriesPoint(point.Key, pointValue.Value));
                }
            }

            return output;
        }

        public IList<RenderedControlActionPoint> GetControlActions()
        {
            return _dataPoints.OrderBy(point => point.Key).Select(point => point.Value.ControlAction.HasValue ? new RenderedControlActionPoint(point.Key, point.Value.ControlAction.Value) : null).OfType<RenderedControlActionPoint>().ToList();
        }

        public IList<DateTime> GetDates()
        {
            return _dataPoints.Select(x => x.Key.Date).Distinct().ToList();
        }

        public bool Cull(TimeSpan deleteOlderThan, ICurrentTimeProvider currentTimeProvider)
        {
            var olderPoints = _dataPoints.Keys.Where(x => x < currentTimeProvider.UtcNow.Subtract(deleteOlderThan)).ToList();
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

        public DateTime? GetMinimumDate(Func<TimeSeriesPoint, bool> predicate)
        {
            var eligible = _dataPoints.Where(x => predicate(x.Value)).Select(x => x.Key);
            if (!eligible.Any())
            {
                return null;
            }

            return eligible.Min();
        }

        public DateTime? GetMinimumDate() => GetMinimumDate(_ => true);

        internal bool Set(DateTime dateTime, Action<TimeSeriesPoint> action)
        {
            if (_dataPoints.TryGetValue(dateTime, out var point))
            {
                action(point);
                return true;
            }

            return false;
        }

        internal void Set(Action<TimeSeriesPoint> action)
        {
            _dataPoints.Values.Each(action);
        }

        public IEnumerator<TimeSeriesPoint> GetEnumerator()
        {
            return _dataPoints.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
