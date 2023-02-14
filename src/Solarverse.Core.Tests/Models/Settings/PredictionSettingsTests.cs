namespace Solarverse.Core.Tests.Models.Settings
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class PredictionSettingsTests
    {
        private PredictionSettings _testClass;

        public PredictionSettingsTests()
        {
            _testClass = new PredictionSettings();
        }

        [Fact]
        public void CanSetAndGetMethodName()
        {
            // Arrange
            var testValue = "TestValue1657231764";

            // Act
            _testClass.MethodName = testValue;

            // Assert
            _testClass.MethodName.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetNumberOfDays()
        {
            // Arrange
            var testValue = 167459139;

            // Act
            _testClass.NumberOfDays = testValue;

            // Assert
            _testClass.NumberOfDays.Should().Be(testValue);
        }
    }
}