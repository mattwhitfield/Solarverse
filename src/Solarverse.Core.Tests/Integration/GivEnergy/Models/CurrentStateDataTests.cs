namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;
    using Array = Core.Integration.GivEnergy.Models.Array;

    public class CurrentStateDataTests
    {
        private CurrentStateData _testClass;

        public CurrentStateDataTests()
        {
            _testClass = new CurrentStateData();
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
        public void CanSetAndGetSolar()
        {
            // Arrange
            var testValue = new Solar { Power = 666605446, Arrays = new List<Array>() };

            // Act
            _testClass.Solar = testValue;

            // Assert
            _testClass.Solar.Should().BeSameAs(testValue);
        }

        [Fact]
        public void CanSetAndGetBattery()
        {
            // Arrange
            var testValue = new Battery { Percent = 1807820163, Power = 1259691763, Temperature = 1502096424.84 };

            // Act
            _testClass.Battery = testValue;

            // Assert
            _testClass.Battery.Should().BeSameAs(testValue);
        }
    }
}