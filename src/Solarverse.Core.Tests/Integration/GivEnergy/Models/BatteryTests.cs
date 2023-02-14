namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class BatteryTests
    {
        private Battery _testClass;

        public BatteryTests()
        {
            _testClass = new Battery();
        }

        [Fact]
        public void CanSetAndGetPercent()
        {
            // Arrange
            var testValue = 1891602431;

            // Act
            _testClass.Percent = testValue;

            // Assert
            _testClass.Percent.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPower()
        {
            // Arrange
            var testValue = 1714296450;

            // Act
            _testClass.Power = testValue;

            // Assert
            _testClass.Power.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetTemperature()
        {
            // Arrange
            var testValue = 1033169360.85;

            // Act
            _testClass.Temperature = testValue;

            // Assert
            _testClass.Temperature.Should().Be(testValue);
        }
    }
}