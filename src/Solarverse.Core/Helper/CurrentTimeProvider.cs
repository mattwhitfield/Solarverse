namespace Solarverse.Core.Helper
{
    public class CurrentTimeProvider : ICurrentTimeProvider
    {
        public DateTime LocalNow => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime CurrentPeriodStartUtc => Period.HalfHourly.GetLast(DateTime.UtcNow);
    }
}
