namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class CommunicationsDeviceListTests
    {
        private CommunicationsDeviceList _testClass;

        public CommunicationsDeviceListTests()
        {
            _testClass = new CommunicationsDeviceList();
        }

        [Fact]
        public void CanSetAndGetCommunicationsDevices()
        {
            // Arrange
            var testValue = new List<CommunicationsDevice>();

            // Act
            _testClass.CommunicationsDevices = testValue;

            // Assert
            _testClass.CommunicationsDevices.Should().BeSameAs(testValue);
        }
    }
}