namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class PowerTests
    {
        private Power _testClass;

        public PowerTests()
        {
            _testClass = new Power();
        }

        [Fact]
        public void CanSetAndGetBattery()
        {
            // Arrange
            var testValue = new PowerBattery { Percent = 1371313535.1299999 };

            // Act
            _testClass.Battery = testValue;

            // Assert
            _testClass.Battery.Should().BeSameAs(testValue);
        }
    }
}