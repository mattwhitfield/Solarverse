using Newtonsoft.Json;

namespace Solarverse.Core.Data
{
    public class TimeSeriesPoint
    {
        public TimeSeriesPoint(DateTime time)
        {
            Time = time;
        }

        public DateTime Time { get; }

        public double? ForecastSolarKwh { get; set; }

        public double? ActualSolarKwh { get; set; }

        public double? ForecastConsumptionKwh { get; set; }

        public double? ActualConsumptionKwh { get; set; }

        public double? IncomingRate { get; set; }

        public double? OutgoingRate { get; set; }

        public double? ActualBatteryPercentage { get; set; }

        public double? ForecastBatteryPercentage { get; set; }

        public ControlAction? ControlAction { get; set; }

        public bool IsDischargeTarget { get; set; }

        public double? RequiredBatteryPowerKwh { get; set; }

        [JsonIgnore]
        public double? RequiredPowerKwh => ForecastSolarKwh.HasValue && ForecastConsumptionKwh.HasValue ? ForecastConsumptionKwh.Value - ForecastSolarKwh.Value : null;

        [JsonIgnore]
        public double? ExcessPowerKwh => ForecastSolarKwh.HasValue && ForecastConsumptionKwh.HasValue ? ForecastSolarKwh.Value - ForecastConsumptionKwh.Value : null;
    }
}
