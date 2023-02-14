namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class PowerBatteryTests
    {
        private PowerBattery _testClass;

        public PowerBatteryTests()
        {
            _testClass = new PowerBattery();
        }

        [Fact]
        public void CanSetAndGetPercent()
        {
            // Arrange
            var testValue = 1992501925.92;

            // Act
            _testClass.Percent = testValue;

            // Assert
            _testClass.Percent.Should().Be(testValue);
        }
    }
}