using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using System.Collections;

namespace Solarverse.Core.Data
{
    public class ForecastTimeSeries : IEnumerable<ForecastTimeSeriesPoint>
    {
        private readonly List<ForecastTimeSeriesPoint> _points;
        private readonly ILogger _logger;
        private readonly double _efficiency;
        private readonly double _maxChargeKwhPerPeriod;
        private readonly double _capacity;
        private readonly int _reserve;

        public double Efficiency => _efficiency;

        public double MaxChargeKwhPerPeriod => _maxChargeKwhPerPeriod;

        public double Capacity => _capacity;

        public int Reserve => _reserve;

        public ForecastTimeSeries(IEnumerable<TimeSeriesPoint> points, ILogger logger, ICurrentDataService currentDataService, IConfigurationProvider configurationProvider)
        {
            _points = points.OrderByDescending(x => x.Time).Select(x => new ForecastTimeSeriesPoint(x, this)).ToList();
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i].Previous = i > 0 ? _points[i - 1] : null;
                _points[i].Next = i < _points.Count - 1 ? _points[i + 1] : null;
            }

            _logger = logger;
            _efficiency = configurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            _maxChargeKwhPerPeriod = currentDataService.CurrentState.MaxChargeRateKw * 0.5 * _efficiency;
            _capacity = configurationProvider.Configuration.Battery.CapacityKwh ?? 5;
            _reserve = currentDataService.CurrentState.BatteryReserve;
        }

        public IEnumerator<ForecastTimeSeriesPoint> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RunActionOnSolarExcessStarts(Action<(ForecastTimeSeriesPoint Point, IList<ForecastTimeSeriesPoint> PriorPoints)> action)
        {
            for (int i = _points.Count - 2; i > 0; i--)
            {
                var current = _points[i];
                var previous = current.Previous;
                var next = current.Next;

                if (previous != null &&
                    next != null &&
                    previous.ForecastBatteryPercentage < 100 &&
                    current.ForecastBatteryPercentage >= 100 &&
                    next.ForecastBatteryPercentage >= 100)
                {
                    var priorPoints = _points
                            .Where(x => x.Time < current.Time)
                            .OrderBy(x => x.Time)
                            .TakeWhile(x => x.ForecastBatteryPercentage > _reserve && x.ForecastBatteryPercentage < 100)
                            .ToList();

                    action((_points[i], priorPoints));
                }
            }
        }

        public void RunActionOnDischargeStartPeriods(string passName, Action<(ForecastTimeSeriesPoint Point, double PointPercentRequired, IList<ForecastTimeSeriesPoint> DischargePoints)> action)
        {
            var lastPoint = _points.First();
            foreach (var point in _points.Skip(1))
            {
                if (lastPoint.IsDischargeTarget &&
                    !point.IsDischargeTarget &&
                    lastPoint.RequiredBatteryPowerKwh.HasValue)
                {
                    var lastPointPercent =
                        (((lastPoint.RequiredBatteryPowerKwh.Value / Efficiency) / Capacity) * 100) + Reserve;
                    if (lastPointPercent > 100)
                    {
                        lastPointPercent = 100;
                    }

                    if (lastPoint.ForecastBatteryPercentage >= lastPointPercent)
                    {
                        _logger.LogInformation($"({passName}) Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh, {lastPointPercent:N1}% but has forecast of {lastPoint.ForecastBatteryPercentage:N1}% battery - no charge needed");
                    }
                    else
                    {
                        _logger.LogInformation($"({passName}) Point at {lastPoint.Time} is the start of a discharge period, requiring {lastPoint.RequiredBatteryPowerKwh.Value:N2} kWh, {lastPointPercent:N1}%");

                        var dischargePoints = _points
                            .Where(x => x.Time >= lastPoint.Time)
                            .OrderBy(x => x.Time)
                            .TakeWhile(x => x.IsDischargeTarget)
                            .ToList();

                        action((lastPoint, lastPointPercent, dischargePoints));
                    }
                }

                lastPoint = point;
            }
        }
    }
}
