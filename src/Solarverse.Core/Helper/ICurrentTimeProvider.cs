namespace Solarverse.Core.Helper
{
    public interface ICurrentTimeProvider
    {
        DateTime LocalNow { get; }

        DateTime UtcNow { get; }

        TimeSpan Offset { get; }

        DateTime ToLocalTime(DateTime utcTime);

        DateTime FromLocalTime(DateTime localTime);

        TimeSpan ToLocalTime(TimeSpan utcTime);

        DateTime CurrentPeriodStartUtc { get; }
    }
}