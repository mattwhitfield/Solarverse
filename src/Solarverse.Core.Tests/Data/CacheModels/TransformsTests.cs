namespace Solarverse.Core.Tests.Data.CacheModels.Transforms
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Data.CacheModels;
    using Solarverse.Core.Data.CacheModels.Transforms;
    using Solarverse.Core.Models;
    using Xunit;

    public static class TransformsTests
    {
        [Fact]
        public static void CanCallFromCacheWithHouseholdConsumptionCache()
        {
            // Arrange
            var source = new HouseholdConsumptionCache { ContainsInterpolatedPoints = true };

            // Act
            var result = source.FromCache();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallFromCacheWithHouseholdConsumptionCacheWithNullSource()
        {
            FluentActions.Invoking(() => default(HouseholdConsumptionCache).FromCache()).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CanCallToCacheWithHouseholdConsumption()
        {
            // Arrange
            var source = new HouseholdConsumption(false, true, new[] { new HouseholdConsumptionDataPoint(DateTime.UtcNow, 128005101.17999999, 803190366.99, 105452560.62, 947007090.81, 2072714704.83, 1037851196.58, 585714669.21), new HouseholdConsumptionDataPoint(DateTime.UtcNow, 1351252816.65, 934410449.61, 180124386.75, 1561561225.29, 1645279766.46, 585406746.54, 1769607393.84), new HouseholdConsumptionDataPoint(DateTime.UtcNow, 1006069680.5, 1123390522.98, 1777134958.83, 574808666.85, 207839721.87, 770092156.35, 1167481464.93) });

            // Act
            var result = source.ToCache();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallToCacheWithHouseholdConsumptionWithNullSource()
        {
            FluentActions.Invoking(() => default(HouseholdConsumption).ToCache()).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CanCallFromCacheWithIEnumerableOfSolarForecastPointCache()
        {
            // Arrange
            var source = new[] { new SolarForecastPointCache { Time = DateTime.UtcNow, PVEstimate = 1702457393.67 }, new SolarForecastPointCache { Time = DateTime.UtcNow, PVEstimate = 1332256520.43 }, new SolarForecastPointCache { Time = DateTime.UtcNow, PVEstimate = 292780957.59 } };

            // Act
            var result = source.FromCache();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallFromCacheWithIEnumerableOfSolarForecastPointCacheWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<SolarForecastPointCache>).FromCache()).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CanCallToCacheWithSolarForecast()
        {
            // Arrange
            var source = new SolarForecast(false, new[] { new SolarForecastPoint(DateTime.UtcNow, 32611544.46), new SolarForecastPoint(DateTime.UtcNow, 1817155381.1399999), new SolarForecastPoint(DateTime.UtcNow, 1164590842.14) });

            // Act
            var result = source.ToCache();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallToCacheWithSolarForecastWithNullSource()
        {
            FluentActions.Invoking(() => default(SolarForecast).ToCache()).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CanCallFromCacheWithIEnumerableOfTariffRateCache()
        {
            // Arrange
            var source = new[] { new TariffRateCache { Value = 1578901423.77, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow }, new TariffRateCache { Value = 1318277917.44, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow }, new TariffRateCache { Value = 1169531459.91, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow } };

            // Act
            var result = source.FromCache();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallFromCacheWithIEnumerableOfTariffRateCacheWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<TariffRateCache>).FromCache()).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CanCallToCacheWithIEnumerableOfTariffRate()
        {
            // Arrange
            var source = new[] { new TariffRate(783050786.1, DateTime.UtcNow, DateTime.UtcNow), new TariffRate(1049769936.27, DateTime.UtcNow, DateTime.UtcNow), new TariffRate(1058620700.61, DateTime.UtcNow, DateTime.UtcNow) };

            // Act
            var result = source.ToCache();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallToCacheWithIEnumerableOfTariffRateWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<TariffRate>).ToCache()).Should().Throw<ArgumentNullException>();
        }
    }
}