namespace Solarverse.Core.Helper
{
    public interface ICurrentTimeProvider
    {
        DateTime LocalNow { get; }

        DateTime UtcNow { get; }

        TimeSpan Offset { get; }

        DateTime ToLocalTime(DateTime utcTime);

        TimeSpan ToLocalTime(TimeSpan utcTime);

        DateTime CurrentPeriodStartUtc { get; }
    }
}