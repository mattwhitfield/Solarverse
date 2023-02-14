namespace Solarverse.Core.Tests.Data.CacheModels
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data.CacheModels;
    using Xunit;

    public class SolarForecastPointCacheTests
    {
        private SolarForecastPointCache _testClass;

        public SolarForecastPointCacheTests()
        {
            _testClass = new SolarForecastPointCache();
        }

        [Fact]
        public void CanSetAndGetTime()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.Time = testValue;

            // Assert
            _testClass.Time.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPVEstimate()
        {
            // Arrange
            var testValue = 2000661726.69;

            // Act
            _testClass.PVEstimate = testValue;

            // Assert
            _testClass.PVEstimate.Should().Be(testValue);
        }
    }
}