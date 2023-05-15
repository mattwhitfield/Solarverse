using Solarverse.Core.Helper;

namespace Solarverse.Core.Data
{
    public class ForecastTimeSeriesPoint
    {
        private readonly TimeSeriesPoint _point;
        private readonly ForecastTimeSeries _owningSeries;

        public ForecastTimeSeriesPoint? Next { get; set; }

        public ForecastTimeSeriesPoint? Previous { get; set; }

        public ForecastTimeSeriesPoint(TimeSeriesPoint point, ForecastTimeSeries owningSeries)
        {
            _point = point;
            _owningSeries = owningSeries;
        }

        public DateTime Time => _point.Time;

        public double? ForecastSolarKwh => _point.ForecastSolarKwh;

        public double? ForecastConsumptionKwh => _point.ForecastConsumptionKwh;

        public double? IncomingRate => _point.IncomingRate;

        public double? OutgoingRate => _point.OutgoingRate;

        public double? ForecastBatteryPercentage => _point.ForecastBatteryPercentage;

        public double? ForecastBatteryKwh => PercentageToKwh(ForecastBatteryPercentage);

        private double? PercentageToKwh(double? percentage)
        {
            if (!percentage.HasValue)
            {
                return null;
            }

            return Math.Max((((percentage.Value - _owningSeries.Reserve) / 100.0) * _owningSeries.Capacity) * _owningSeries.Efficiency, 0);
        }

        public ControlAction? ControlAction { get => _point.ControlAction; set => _point.ControlAction = value; }

        public bool IsDischargeTarget { get => _point.IsDischargeTarget; set => _point.IsDischargeTarget = value; }

        public double? RequiredBatteryPowerKwh { get => _point.RequiredBatteryPowerKwh; set => _point.RequiredBatteryPowerKwh = value; }

        public bool IsFuture(ICurrentTimeProvider currentTimeProvider) => _point.IsFuture(currentTimeProvider);

        public double? RequiredPowerKwh => _point.RequiredPowerKwh;

        public double? ExcessPowerKwh => _point.ExcessPowerKwh;
    }
}
