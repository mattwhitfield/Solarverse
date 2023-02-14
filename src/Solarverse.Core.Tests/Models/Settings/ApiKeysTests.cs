namespace Solarverse.Core.Tests.Models.Settings
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class ApiKeysTests
    {
        private ApiKeys _testClass;

        public ApiKeysTests()
        {
            _testClass = new ApiKeys();
        }

        [Fact]
        public void CanSetAndGetOctopus()
        {
            // Arrange
            var testValue = "TestValue860296106";

            // Act
            _testClass.Octopus = testValue;

            // Assert
            _testClass.Octopus.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetGivEnergy()
        {
            // Arrange
            var testValue = "TestValue882515294";

            // Act
            _testClass.GivEnergy = testValue;

            // Assert
            _testClass.GivEnergy.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetSolcast()
        {
            // Arrange
            var testValue = "TestValue1933046663";

            // Act
            _testClass.Solcast = testValue;

            // Assert
            _testClass.Solcast.Should().Be(testValue);
        }
    }
}