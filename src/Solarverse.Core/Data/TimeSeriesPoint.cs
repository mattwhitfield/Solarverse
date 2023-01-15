namespace Solarverse.Core.Data
{
    public class TimeSeriesPoint
    {
        public double? PVForecastKwh { get; set; }

        public double? ConsumptionForecastKwh { get; set; }

        public double? IncomingRate { get; set; }

        public double? OutgoingRate { get; set; }

        public ControlAction? ControlAction { get; set; }

        public double? CostWithoutStorage => IncomingRate.HasValue && RequiredPowerKwh.HasValue ? IncomingRate.Value * Math.Max(RequiredPowerKwh.Value, 0) : null;

        public double? RequiredPowerKwh => PVForecastKwh.HasValue && ConsumptionForecastKwh.HasValue ? ConsumptionForecastKwh.Value - PVForecastKwh.Value : null;

        public double? ExcessPowerKwh => PVForecastKwh.HasValue && ConsumptionForecastKwh.HasValue ? PVForecastKwh.Value - ConsumptionForecastKwh.Value : null;
    }
}
