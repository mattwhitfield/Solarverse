namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;
    using Array = Core.Integration.GivEnergy.Models.Array;

    public class SolarTests
    {
        private Solar _testClass;

        public SolarTests()
        {
            _testClass = new Solar();
        }

        [Fact]
        public void CanSetAndGetPower()
        {
            // Arrange
            var testValue = 448145074;

            // Act
            _testClass.Power = testValue;

            // Assert
            _testClass.Power.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetArrays()
        {
            // Arrange
            var testValue = new List<Array>();

            // Act
            _testClass.Arrays = testValue;

            // Assert
            _testClass.Arrays.Should().BeSameAs(testValue);
        }
    }
}