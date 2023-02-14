namespace Solarverse.Core.Tests.Models.Settings
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class MeterPointConfigurationTests
    {
        private MeterPointConfiguration _testClass;

        public MeterPointConfigurationTests()
        {
            _testClass = new MeterPointConfiguration();
        }

        [Fact]
        public void CanSetAndGetMPAN()
        {
            // Arrange
            var testValue = "TestValue428138500";

            // Act
            _testClass.MPAN = testValue;

            // Assert
            _testClass.MPAN.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetTariffName()
        {
            // Arrange
            var testValue = "TestValue392895120";

            // Act
            _testClass.TariffName = testValue;

            // Assert
            _testClass.TariffName.Should().Be(testValue);
        }
    }
}