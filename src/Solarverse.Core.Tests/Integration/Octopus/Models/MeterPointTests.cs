namespace Solarverse.Core.Tests.Integration.Octopus.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.Octopus.Models;
    using Xunit;

    public class MeterPointTests
    {
        private MeterPoint _testClass;

        public MeterPointTests()
        {
            _testClass = new MeterPoint();
        }

        [Fact]
        public void CanSetAndGetGridSupplyPoint()
        {
            // Arrange
            var testValue = "TestValue1139104294";

            // Act
            _testClass.GridSupplyPoint = testValue;

            // Assert
            _testClass.GridSupplyPoint.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetMPAN()
        {
            // Arrange
            var testValue = "TestValue1662233785";

            // Act
            _testClass.MPAN = testValue;

            // Assert
            _testClass.MPAN.Should().Be(testValue);
        }
    }
}