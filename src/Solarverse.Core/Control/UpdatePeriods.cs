using Solarverse.Core.Helper;

namespace Solarverse.Core.Control
{
    public static class UpdatePeriods
    {
        public static Period TariffUpdates { get; } = new Period(TimeSpan.FromMinutes(2));

        public static Period SolarForecastUpdates { get; } = new Period(TimeSpan.FromHours(2));

        public static Period ConsumptionCacheUpdates { get; } = new Period(TimeSpan.FromDays(1));

        public static Period ConsumptionUpdates { get; } = new Period(TimeSpan.FromMinutes(2));
    }
}
