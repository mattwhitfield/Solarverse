namespace Solarverse.Core.Tests.Helper
{
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Solarverse.Core.Helper;
    using System;
    using System.Linq;
    using Xunit;

    public static class BucketizerTests
    {
        [Fact]
        public static void CanCallBucketizeWithTooSmallAmount()
        {
            // Arrange
            var points = new[] {
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 4, IncomingRate = 100 }
            };
            var kwhPerPeriod = 5;

            // Act
            var result = Bucketizer.Bucketize(points.Select(x => new ForecastTimeSeriesPoint(x, null)), kwhPerPeriod);

            // Assert
            result.Should().Equal(100);
        }

        [Fact]
        public static void CanCallBucketizeWithExactSizes()
        {
            // Arrange
            var points = new[] { 
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 4, IncomingRate = 100 }, 
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 6, IncomingRate = 50 }, 
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 8, IncomingRate = 25 }
            };
            var kwhPerPeriod = 2;

            // Act
            var result = Bucketizer.Bucketize(points.Select(x => new ForecastTimeSeriesPoint(x, null)), kwhPerPeriod);

            // Assert
            result.Should().Equal(100, 100, 50, 50, 50, 25, 25, 25, 25);
        }

        [Fact]
        public static void CanCallBucketizeWithOddSizes()
        {
            // Arrange
            var points = new[] {
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 3, IncomingRate = 100 },
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 4, IncomingRate = 50 },
                new TimeSeriesPoint(DateTime.UtcNow) { ForecastSolarKwh = 0, ForecastConsumptionKwh = 2, IncomingRate = 10 }
            };
            var kwhPerPeriod = 2;

            // Act
            var result = Bucketizer.Bucketize(points.Select(x => new ForecastTimeSeriesPoint(x, null)), kwhPerPeriod);

            // Assert
            result.Should().Equal(100, 75, 50, 30, 10);
        }
    }
}