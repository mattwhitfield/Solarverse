namespace Solarverse.Core.Helper
{
    public interface ICurrentTimeProvider
    {
        DateTime LocalNow { get; }

        DateTime UtcNow { get; }

        DateTime CurrentPeriodStartUtc { get; }
    }
}