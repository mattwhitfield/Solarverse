namespace Solarverse.Core.Tests.Data.CacheModels
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data.CacheModels;
    using Xunit;

    public class TariffRateCacheTests
    {
        private TariffRateCache _testClass;

        public TariffRateCacheTests()
        {
            _testClass = new TariffRateCache();
        }

        [Fact]
        public void CanSetAndGetValue()
        {
            // Arrange
            var testValue = 1621000052.1;

            // Act
            _testClass.Value = testValue;

            // Assert
            _testClass.Value.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetValidFrom()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.ValidFrom = testValue;

            // Assert
            _testClass.ValidFrom.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetValidTo()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.ValidTo = testValue;

            // Assert
            _testClass.ValidTo.Should().Be(testValue);
        }
    }
}