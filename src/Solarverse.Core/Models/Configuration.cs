namespace Solarverse.Core.Models
{
    public class Configuration
    {
        public ApiKeys? ApiKeys { get; set; }

        public MeterPointConfiguration? IncomingMeter { get; set; }

        public MeterPointConfiguration? OutgoingMeter { get; set; }

        public BatterySettings? Battery { get; set; }

        public string? SolcastSiteId { get; set; }
    }
}