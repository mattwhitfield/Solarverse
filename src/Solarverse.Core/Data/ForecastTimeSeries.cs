using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using System.Collections;

namespace Solarverse.Core.Data
{
    public class ForecastTimeSeries : IEnumerable<TimeSeriesPoint>
    {
        private readonly List<TimeSeriesPoint> _points;
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
            _points = points.OrderByDescending(x => x.Time).ToList();
            _logger = logger;
            _efficiency = configurationProvider.Configuration.Battery.EfficiencyFactor ?? 0.85;
            _maxChargeKwhPerPeriod = currentDataService.CurrentState.MaxChargeRateKw * 0.5 * _efficiency;
            _capacity = configurationProvider.Configuration.Battery.CapacityKwh ?? 5;
            _reserve = currentDataService.CurrentState.BatteryReserve;
        }

        public IEnumerator<TimeSeriesPoint> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RunActionOnDischargeStartPeriods(string passName, Action<(TimeSeriesPoint Point, double PointPercentRequired, IList<TimeSeriesPoint> DischargePoints)> action)
        {
            var lastPoint = _points.First();
            foreach (var point in _points.Skip(1))
            {
                if (lastPoint.ControlAction == ControlAction.Discharge &&
                    point.ControlAction != ControlAction.Discharge &&
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
                            .TakeWhile(x => x.ControlAction == ControlAction.Discharge && x.ExcessPowerKwh < 0)
                            .ToList();

                        action((lastPoint, lastPointPercent, dischargePoints));
                    }
                }

                lastPoint = point;
            }
        }
    }
}
