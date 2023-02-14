namespace Solarverse.Core.Tests.Integration.Octopus.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.Octopus.Models;
    using Xunit;

    public class AgileRateTests
    {
        private AgileRate _testClass;

        public AgileRateTests()
        {
            _testClass = new AgileRate();
        }

        [Fact]
        public void CanSetAndGetValue()
        {
            // Arrange
            var testValue = 1183897493.46;

            // Act
            _testClass.Value = testValue;

            // Assert
            _testClass.Value.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetValidFrom()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.ValidFrom = testValue;

            // Assert
            _testClass.ValidFrom.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetValidTo()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.ValidTo = testValue;

            // Assert
            _testClass.ValidTo.Should().Be(testValue);
        }
    }
}