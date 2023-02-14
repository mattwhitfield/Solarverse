namespace Solarverse.Core.Tests.Models.Settings
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class BatterySettingsTests
    {
        private BatterySettings _testClass;

        public BatterySettingsTests()
        {
            _testClass = new BatterySettings();
        }

        [Fact]
        public void CanSetAndGetCapacityKwh()
        {
            // Arrange
            var testValue = 1408932291.69;

            // Act
            _testClass.CapacityKwh = testValue;

            // Assert
            _testClass.CapacityKwh.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetEfficiencyFactor()
        {
            // Arrange
            var testValue = 5932311.66;

            // Act
            _testClass.EfficiencyFactor = testValue;

            // Assert
            _testClass.EfficiencyFactor.Should().Be(testValue);
        }
    }
}