using Solarverse.Core.Helper;

namespace Solarverse.Core.Control
{
    public static class UpdatePeriods
    {
        public static Period TariffUpdates { get; } =
            new Period(TimeSpan.FromDays(1), TimeSpan.FromHours(17));

        public static Period SolarForecastUpdates { get; } =
            new Period(TimeSpan.FromHours(6));

        public static Period ConsumptionUpdates { get; } =
            new Period(TimeSpan.FromDays(1));
    }
}
