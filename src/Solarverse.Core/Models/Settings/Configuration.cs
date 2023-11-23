namespace Solarverse.Core.Models.Settings
{
    public class Configuration
    {
        public ApiKeys ApiKeys { get; } = new ApiKeys();

        public MeterPointConfiguration IncomingMeter { get; } = new MeterPointConfiguration();

        public MeterPointConfiguration OutgoingMeter { get; } = new MeterPointConfiguration();

        public BatterySettings Battery { get; } = new BatterySettings();

        public PredictionSettings Prediction { get; } = new PredictionSettings();

        public string? SolcastSiteId { get; set; }

        public bool TestMode { get; set; }

        public Guid ApiKey { get; set; }

        public string TimeZoneName { get; set; } = "Europe/London";

        public int NumberOfEVChargePeriodsRequired { get; set; } = 6;

        public int FirstHourForEVCharging { get; set; } = 22;

        public int LastHourForEVCharging { get; set; } = 8;
    }
}