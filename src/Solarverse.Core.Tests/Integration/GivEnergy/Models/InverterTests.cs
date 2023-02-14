namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class InverterTests
    {
        private Inverter _testClass;

        public InverterTests()
        {
            _testClass = new Inverter();
        }

        [Fact]
        public void CanSetAndGetSerialNumber()
        {
            // Arrange
            var testValue = "TestValue625091524";

            // Act
            _testClass.SerialNumber = testValue;

            // Assert
            _testClass.SerialNumber.Should().Be(testValue);
        }
    }
}