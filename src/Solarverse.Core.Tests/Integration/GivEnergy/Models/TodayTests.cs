namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class TodayTests
    {
        private Today _testClass;

        public TodayTests()
        {
            _testClass = new Today();
        }

        [Fact]
        public void CanSetAndGetConsumption()
        {
            // Arrange
            var testValue = 1755815403.87;

            // Act
            _testClass.Consumption = testValue;

            // Assert
            _testClass.Consumption.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetSolar()
        {
            // Arrange
            var testValue = 859258905.12;

            // Act
            _testClass.Solar = testValue;

            // Assert
            _testClass.Solar.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetBattery()
        {
            // Arrange
            var testValue = new TodayBattery { Charge = 396798341.94, Discharge = 1247465729.07 };

            // Act
            _testClass.Battery = testValue;

            // Assert
            _testClass.Battery.Should().BeSameAs(testValue);
        }

        [Fact]
        public void CanSetAndGetGrid()
        {
            // Arrange
            var testValue = new TodayGrid { Import = 108254256.66, Export = 1818579290.22 };

            // Act
            _testClass.Grid = testValue;

            // Assert
            _testClass.Grid.Should().BeSameAs(testValue);
        }
    }
}