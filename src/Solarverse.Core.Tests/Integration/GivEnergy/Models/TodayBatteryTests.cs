namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class TodayBatteryTests
    {
        private TodayBattery _testClass;

        public TodayBatteryTests()
        {
            _testClass = new TodayBattery();
        }

        [Fact]
        public void CanSetAndGetCharge()
        {
            // Arrange
            var testValue = 233959651.2;

            // Act
            _testClass.Charge = testValue;

            // Assert
            _testClass.Charge.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetDischarge()
        {
            // Arrange
            var testValue = 430243218.9;

            // Act
            _testClass.Discharge = testValue;

            // Assert
            _testClass.Discharge.Should().Be(testValue);
        }
    }
}