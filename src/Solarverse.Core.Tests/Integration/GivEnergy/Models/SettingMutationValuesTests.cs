namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class SettingMutationValuesTests
    {
        private SettingMutationValues _testClass;

        public SettingMutationValuesTests()
        {
            _testClass = new SettingMutationValues();
        }

        [Fact]
        public void CanSetAndGetValue()
        {
            // Arrange
            var testValue = new object();

            // Act
            _testClass.Value = testValue;

            // Assert
            _testClass.Value.Should().BeSameAs(testValue);
        }

        [Fact]
        public void CanSetAndGetSuccess()
        {
            // Arrange
            var testValue = false;

            // Act
            _testClass.Success = testValue;

            // Assert
            _testClass.Success.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetMessage()
        {
            // Arrange
            var testValue = "TestValue1626332041";

            // Act
            _testClass.Message = testValue;

            // Assert
            _testClass.Message.Should().Be(testValue);
        }
    }
}