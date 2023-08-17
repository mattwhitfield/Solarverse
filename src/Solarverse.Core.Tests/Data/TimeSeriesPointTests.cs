namespace Solarverse.Core.Tests.Data
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Xunit;

    public class TimeSeriesPointTests
    {
        private TimeSeriesPoint _testClass;
        private DateTime _time;

        public TimeSeriesPointTests()
        {
            _time = DateTime.UtcNow;
            _testClass = new TimeSeriesPoint(_time);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TimeSeriesPoint(_time);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void TimeIsInitializedCorrectly()
        {
            _testClass.Time.Should().Be(_time);
        }

        [Fact]
        public void CanSetAndGetForecastSolarKwh()
        {
            // Arrange
            var testValue = 1028257225.38;

            // Act
            _testClass.ForecastSolarKwh = testValue;

            // Assert
            _testClass.ForecastSolarKwh.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetActualSolarKwh()
        {
            // Arrange
            var testValue = 900945749.88;

            // Act
            _testClass.ActualSolarKwh = testValue;

            // Assert
            _testClass.ActualSolarKwh.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetForecastConsumptionKwh()
        {
            // Arrange
            var testValue = 1578187720.89;

            // Act
            _testClass.ForecastConsumptionKwh = testValue;

            // Assert
            _testClass.ForecastConsumptionKwh.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetActualConsumptionKwh()
        {
            // Arrange
            var testValue = 1628276882.76;

            // Act
            _testClass.ActualConsumptionKwh = testValue;

            // Assert
            _testClass.ActualConsumptionKwh.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetIncomingRate()
        {
            // Arrange
            var testValue = 375459489.9;

            // Act
            _testClass.IncomingRate = testValue;

            // Assert
            _testClass.IncomingRate.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetOutgoingRate()
        {
            // Arrange
            var testValue = 1756785326.67;

            // Act
            _testClass.OutgoingRate = testValue;

            // Assert
            _testClass.OutgoingRate.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetActualBatteryPercentage()
        {
            // Arrange
            var testValue = 2093858369.46;

            // Act
            _testClass.ActualBatteryPercentage = testValue;

            // Assert
            _testClass.ActualBatteryPercentage.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetForecastBatteryPercentage()
        {
            // Arrange
            var testValue = 344474022.42;

            // Act
            _testClass.ForecastBatteryPercentage = testValue;

            // Assert
            _testClass.ForecastBatteryPercentage.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetControlAction()
        {
            // Arrange
            var testValue = new ControlAction?();

            // Act
            _testClass.ControlAction = testValue;

            // Assert
            _testClass.ControlAction.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetRequiredBatteryPowerKwh()
        {
            // Arrange
            var testValue = 1806428338.11;

            // Act
            _testClass.RequiredBatteryPowerKwh = testValue;

            // Assert
            _testClass.RequiredBatteryPowerKwh.Should().Be(testValue);
        }

        [Fact]
        public void CanGetRequiredPowerKwh()
        {
            // Assert
            _testClass.RequiredPowerKwh.Should().BeNull();

            // Act
            _testClass.ForecastSolarKwh = 3;
            _testClass.ForecastConsumptionKwh = 4.5;

            // Assert
            _testClass.RequiredPowerKwh.Should().Be(1.5);
        }

        [Fact]
        public void CanGetExcessPowerKwh()
        {
            // Assert
            _testClass.ExcessPowerKwh.Should().BeNull();

            // Act
            _testClass.ForecastSolarKwh = 6;
            _testClass.ForecastConsumptionKwh = 4.5;

            // Assert
            _testClass.ExcessPowerKwh.Should().Be(1.5);
        }

        [Fact]
        public void CanSetAndGetDischargeTarget()
        {
            // Arrange
            var testValue = TargetType.NoDischargeRequired;

            // Act
            _testClass.Target = testValue;

            // Assert
            _testClass.Target.Should().Be(testValue);
        }
    }
}