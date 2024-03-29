﻿using Newtonsoft.Json;
using Solarverse.Core.Helper;

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

        public TargetType Target { get; set; }

        public bool ShouldChargeEV { get; set; }

        public double? RequiredBatteryPowerKwh { get; set; }

        public bool IsFuture(ICurrentTimeProvider currentTimeProvider) => !ActualConsumptionKwh.HasValue && currentTimeProvider.CurrentPeriodStartUtc < Time;

        public bool ShouldDischarge() =>
            Target == TargetType.TariffBasedDischargeRequired ||
            Target == TargetType.PairingDischargeRequired;

        [JsonIgnore]
        public double? RequiredPowerKwh => ForecastSolarKwh.HasValue && ForecastConsumptionKwh.HasValue ? ForecastConsumptionKwh.Value - ForecastSolarKwh.Value : null;

        [JsonIgnore]
        public double? ExcessPowerKwh => ForecastSolarKwh.HasValue && ForecastConsumptionKwh.HasValue ? ForecastSolarKwh.Value - ForecastConsumptionKwh.Value : null;
    }
}
