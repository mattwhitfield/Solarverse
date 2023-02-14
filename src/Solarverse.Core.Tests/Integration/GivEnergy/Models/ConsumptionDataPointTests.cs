namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class ConsumptionDataPointTests
    {
        private ConsumptionDataPoint _testClass;

        public ConsumptionDataPointTests()
        {
            _testClass = new ConsumptionDataPoint();
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
        public void CanSetAndGetToday()
        {
            // Arrange
            var testValue = new Today { Consumption = 699960719.7, Solar = 905443332.75, Battery = new TodayBattery { Charge = 565678542.33, Discharge = 1789539438.06 }, Grid = new TodayGrid { Import = 1124223601.05, Export = 311272156.8 } };

            // Act
            _testClass.Today = testValue;

            // Assert
            _testClass.Today.Should().BeSameAs(testValue);
        }

        [Fact]
        public void CanSetAndGetPower()
        {
            // Arrange
            var testValue = new Power { Battery = new PowerBattery { Percent = 942218879.58 } };

            // Act
            _testClass.Power = testValue;

            // Assert
            _testClass.Power.Should().BeSameAs(testValue);
        }
    }
}