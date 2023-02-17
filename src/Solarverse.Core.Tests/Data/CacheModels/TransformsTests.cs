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
        public static void CanConvertFromAndToHouseholdConsumption()
        {
            // Arrange
            var source = TestData.Household1;

            // Act
            var result = source.ToCache().FromCache();

            // Assert
            result.Should().BeEquivalentTo(source);
        }

        [Fact]
        public static void CanConvertFromAndToSolarForecast()
        {
            // Arrange
            var source = TestData.SolarForecast;

            // Act
            var result = source.ToCache().FromCache();

            // Assert
            result.Should().BeEquivalentTo(source);
        }

        [Fact]
        public static void CannotCallFromCacheWithIEnumerableOfSolarForecastPointCacheWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<SolarForecastPointCache>).FromCache()).Should().Throw<ArgumentNullException>();
        }
        [Fact]
        public static void CanConvertFromAndToTariffRates()
        {
            // Arrange
            var source = TestData.IncomingRates;

            // Act
            var result = source.ToCache().FromCache();

            // Assert
            result.Should().BeEquivalentTo(source);
        }

        [Fact]
        public static void CannotCallFromCacheWithIEnumerableOfTariffRateCacheWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<TariffRateCache>).FromCache()).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CannotCallToCacheWithIEnumerableOfTariffRateWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<TariffRate>).ToCache()).Should().Throw<ArgumentNullException>();
        }
    }
}