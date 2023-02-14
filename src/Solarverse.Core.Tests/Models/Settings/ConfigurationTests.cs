namespace Solarverse.Core.Tests.Models.Settings
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class ConfigurationTests
    {
        private Configuration _testClass;

        public ConfigurationTests()
        {
            _testClass = new Configuration();
        }

        [Fact]
        public void CanGetApiKeys()
        {
            // Assert
            _testClass.ApiKeys.Should().BeAssignableTo<ApiKeys>();
        }

        [Fact]
        public void CanGetIncomingMeter()
        {
            // Assert
            _testClass.IncomingMeter.Should().BeAssignableTo<MeterPointConfiguration>();
        }

        [Fact]
        public void CanGetOutgoingMeter()
        {
            // Assert
            _testClass.OutgoingMeter.Should().BeAssignableTo<MeterPointConfiguration>();
        }

        [Fact]
        public void CanGetBattery()
        {
            // Assert
            _testClass.Battery.Should().BeAssignableTo<BatterySettings>();
        }

        [Fact]
        public void CanGetPrediction()
        {
            // Assert
            _testClass.Prediction.Should().BeAssignableTo<PredictionSettings>();
        }

        [Fact]
        public void CanSetAndGetSolcastSiteId()
        {
            // Arrange
            var testValue = "TestValue1826961534";

            // Act
            _testClass.SolcastSiteId = testValue;

            // Assert
            _testClass.SolcastSiteId.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetTestMode()
        {
            // Arrange
            var testValue = false;

            // Act
            _testClass.TestMode = testValue;

            // Assert
            _testClass.TestMode.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetApiKey()
        {
            // Arrange
            var testValue = new Guid("044b959f-44f8-432f-ba6d-e9345a7cb268");

            // Act
            _testClass.ApiKey = testValue;

            // Assert
            _testClass.ApiKey.Should().Be(testValue);
        }
    }
}