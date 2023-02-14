namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class SettingTests
    {
        private Setting _testClass;

        public SettingTests()
        {
            _testClass = new Setting();
        }

        [Fact]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = 560991895;

            // Act
            _testClass.Id = testValue;

            // Assert
            _testClass.Id.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = "TestValue51125475";

            // Act
            _testClass.Name = testValue;

            // Assert
            _testClass.Name.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetValidation()
        {
            // Arrange
            var testValue = "TestValue321413062";

            // Act
            _testClass.Validation = testValue;

            // Assert
            _testClass.Validation.Should().Be(testValue);
        }
    }
}