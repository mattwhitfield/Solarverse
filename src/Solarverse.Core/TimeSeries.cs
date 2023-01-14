using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solarverse.Core
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
    }

    public class RenderedTimeSeriesPoint
    {
        public RenderedTimeSeriesPoint(DateTime time, double? value)
        {
            Time = time;
            Value = value;
        }

        public DateTime Time { get; }

        public double? Value { get; }
    }


    public class TimeSeriesPoint
    {
        public double? PVForecastKwh { get; set; }

        public double? ConsumptionForecastKwh { get; set; }

        public double? AgileRatePence { get; set; }

        public double? CostWithoutStorage => AgileRatePence.HasValue && RequiredPowerKwh.HasValue ? AgileRatePence.Value * Math.Max(RequiredPowerKwh.Value, 0) : null;

        public double? RequiredPowerKwh => PVForecastKwh.HasValue && ConsumptionForecastKwh.HasValue ? ConsumptionForecastKwh.Value - PVForecastKwh.Value : null;

        public double? ExcessPowerKwh => PVForecastKwh.HasValue && ConsumptionForecastKwh.HasValue ? PVForecastKwh.Value - ConsumptionForecastKwh.Value : null;
    }
}
