namespace Solarverse.Core.Models
{
    public class Configuration
    {
        public ApiKeys? ApiKeys { get; set; }

        public MeterPoint? IncomingMeter { get; set; }

        public MeterPoint? OutgoingMeter { get; set; }

        public BatterySettings? Battery { get; set; }

        public string SolcastSiteId { get; set; }
    }

    public class BatterySettings
    {
        public double? CapacityKwh { get; set; }

        public double? EfficiencyFactor { get; set; }
    }

    public class MeterPoint
    {
        public string? MPAN { get; set; }

        public string? TariffName { get; set; }
    }

    public class ApiKeys
    {
        public string? Octopus { get; set; }

        public string? GivEnergy { get; set; }

        public string? Solcast { get; set; }
    }
}