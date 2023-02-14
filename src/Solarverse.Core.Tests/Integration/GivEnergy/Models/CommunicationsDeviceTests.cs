namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class CommunicationsDeviceTests
    {
        private CommunicationsDevice _testClass;

        public CommunicationsDeviceTests()
        {
            _testClass = new CommunicationsDevice();
        }

        [Fact]
        public void CanSetAndGetSerialNumber()
        {
            // Arrange
            var testValue = "TestValue1451307741";

            // Act
            _testClass.SerialNumber = testValue;

            // Assert
            _testClass.SerialNumber.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetCommunicationsType()
        {
            // Arrange
            var testValue = "TestValue1814385797";

            // Act
            _testClass.CommunicationsType = testValue;

            // Assert
            _testClass.CommunicationsType.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetInverter()
        {
            // Arrange
            var testValue = new Inverter { SerialNumber = "TestValue2063364375" };

            // Act
            _testClass.Inverter = testValue;

            // Assert
            _testClass.Inverter.Should().BeSameAs(testValue);
        }
    }
}