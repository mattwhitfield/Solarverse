namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;

    public class PeriodTests
    {
        const int HalfHourly = 1800;
        const int SixHourly = 86400 / 4;
        const int Daily = 86400;
        const int FivePm = 3600 * 17;

        [Theory]
        [InlineData(HalfHourly, 0, 0, 1800)]
        [InlineData(HalfHourly, 60, 0, 60)]
        [InlineData(HalfHourly, 0, 1801, 3600)]
        [InlineData(HalfHourly, 1740, 0, 1740)]
        [InlineData(HalfHourly, 1740, 1739, 1740)]
        [InlineData(HalfHourly, 1740, 1740, 3540)]
        [InlineData(HalfHourly, 0, 84600, 86400)]
        [InlineData(SixHourly, 0, 0, 21600)]
        [InlineData(SixHourly, 0, 1800, 21600)]
        [InlineData(SixHourly, 0, 21600, 43200)]
        [InlineData(Daily, FivePm, 21600, FivePm)]
        [InlineData(Daily, FivePm, FivePm, FivePm + Daily)]
        public void CanCallGetNext(int periodSeconds, int offsetSeconds, int currentSeconds, int nextExpectedSeconds)
        {
            var baseDate = DateTime.UtcNow.Date;

            var instance = new Period(TimeSpan.FromSeconds(periodSeconds), TimeSpan.FromSeconds(offsetSeconds));

            var next = instance.GetNext(baseDate.AddSeconds(currentSeconds));

            next.Should().BeCloseTo(baseDate.AddSeconds(nextExpectedSeconds), TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(HalfHourly, 1740, 0, -60)]
        [InlineData(HalfHourly, 1740, 1739, -60)]
        [InlineData(HalfHourly, 1740, 1740, 1740)]
        [InlineData(HalfHourly, 0, 84600, 86400 - HalfHourly)]
        [InlineData(SixHourly, 0, 0, 0)]
        [InlineData(SixHourly, 0, 1800, 0)]
        [InlineData(SixHourly, 0, 21600, 21600)]
        [InlineData(Daily, FivePm, 21600, FivePm - Daily)]
        [InlineData(Daily, FivePm, FivePm, FivePm)]
        public void CanCallGetLast(int periodSeconds, int offsetSeconds, int currentSeconds, int nextExpectedSeconds)
        {
            var baseDate = DateTime.UtcNow.Date;

            var instance = new Period(TimeSpan.FromSeconds(periodSeconds), TimeSpan.FromSeconds(offsetSeconds));

            var next = instance.GetLast(baseDate.AddSeconds(currentSeconds));

            next.Should().BeCloseTo(baseDate.AddSeconds(nextExpectedSeconds), TimeSpan.FromSeconds(1));
        }
    }
}