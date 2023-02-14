namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;
    using Array = Core.Integration.GivEnergy.Models.Array;

    public class ArrayTests
    {
        private Array _testClass;

        public ArrayTests()
        {
            _testClass = new Array();
        }

        [Fact]
        public void CanSetAndGetArrayNumber()
        {
            // Arrange
            var testValue = 1879534041;

            // Act
            _testClass.ArrayNumber = testValue;

            // Assert
            _testClass.ArrayNumber.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetVoltage()
        {
            // Arrange
            var testValue = 1495247996.8799999;

            // Act
            _testClass.Voltage = testValue;

            // Assert
            _testClass.Voltage.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetCurrent()
        {
            // Arrange
            var testValue = 196302275.73;

            // Act
            _testClass.Current = testValue;

            // Assert
            _testClass.Current.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetPower()
        {
            // Arrange
            var testValue = 1351980997;

            // Act
            _testClass.Power = testValue;

            // Assert
            _testClass.Power.Should().Be(testValue);
        }
    }
}