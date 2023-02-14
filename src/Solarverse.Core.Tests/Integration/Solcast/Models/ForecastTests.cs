namespace Solarverse.Core.Tests.Integration.Solcast.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.Solcast.Models;
    using Xunit;

    public class ForecastTests
    {
        private Forecast _testClass;

        public ForecastTests()
        {
            _testClass = new Forecast();
        }

        [Fact]
        public void CanSetAndGetPVEstimate()
        {
            // Arrange
            var testValue = 953497199.16;

            // Act
            _testClass.PVEstimate = testValue;

            // Assert
            _testClass.PVEstimate.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPVEstimate10thPercentile()
        {
            // Arrange
            var testValue = 245683978.65;

            // Act
            _testClass.PVEstimate10thPercentile = testValue;

            // Assert
            _testClass.PVEstimate10thPercentile.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPVEstimate90thPercentile()
        {
            // Arrange
            var testValue = 925575382.71;

            // Act
            _testClass.PVEstimate90thPercentile = testValue;

            // Assert
            _testClass.PVEstimate90thPercentile.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPeriodEnd()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.PeriodEnd = testValue;

            // Assert
            _testClass.PeriodEnd.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPeriodType()
        {
            // Arrange
            var testValue = "TestValue1330670834";

            // Act
            _testClass.PeriodType = testValue;

            // Assert
            _testClass.PeriodType.Should().Be(testValue);
        }
    }
}