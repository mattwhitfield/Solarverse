namespace Solarverse.Core.Tests.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class HouseholdConsumptionDataPointTests
    {
        private HouseholdConsumptionDataPoint _testClass;
        private DateTime _time;
        private double _consumption;
        private double _solar;
        private double _import;
        private double _export;
        private double _cahrge;
        private double _discharge;
        private double _batteryPercentage;

        public HouseholdConsumptionDataPointTests()
        {
            _time = DateTime.UtcNow;
            _consumption = 1858353041.16;
            _solar = 1900893130.29;
            _import = 529640084.15999997;
            _export = 1982726620.3799999;
            _cahrge = 1260934035.57;
            _discharge = 270184425.39;
            _batteryPercentage = 406749953.61;
            _testClass = new HouseholdConsumptionDataPoint(_time, _consumption, _solar, _import, _export, _cahrge, _discharge, _batteryPercentage);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new HouseholdConsumptionDataPoint(_time, _consumption, _solar, _import, _export, _cahrge, _discharge, _batteryPercentage);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void TimeIsInitializedCorrectly()
        {
            _testClass.Time.Should().Be(_time);
        }

        [Fact]
        public void ConsumptionIsInitializedCorrectly()
        {
            _testClass.Consumption.Should().Be(_consumption);
        }

        [Fact]
        public void CanSetAndGetConsumption()
        {
            // Arrange
            var testValue = 1681440347.07;

            // Act
            _testClass.Consumption = testValue;

            // Assert
            _testClass.Consumption.Should().Be(testValue);
        }

        [Fact]
        public void SolarIsInitializedCorrectly()
        {
            _testClass.Solar.Should().Be(_solar);
        }

        [Fact]
        public void CanSetAndGetSolar()
        {
            // Arrange
            var testValue = 274265619.21;

            // Act
            _testClass.Solar = testValue;

            // Assert
            _testClass.Solar.Should().Be(testValue);
        }

        [Fact]
        public void ImportIsInitializedCorrectly()
        {
            _testClass.Import.Should().Be(_import);
        }

        [Fact]
        public void CanSetAndGetImport()
        {
            // Arrange
            var testValue = 1008113735.97;

            // Act
            _testClass.Import = testValue;

            // Assert
            _testClass.Import.Should().Be(testValue);
        }

        [Fact]
        public void ExportIsInitializedCorrectly()
        {
            _testClass.Export.Should().Be(_export);
        }

        [Fact]
        public void CanSetAndGetExport()
        {
            // Arrange
            var testValue = 286545801.96;

            // Act
            _testClass.Export = testValue;

            // Assert
            _testClass.Export.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetCharge()
        {
            // Arrange
            var testValue = 1023606611.28;

            // Act
            _testClass.Charge = testValue;

            // Assert
            _testClass.Charge.Should().Be(testValue);
        }

        [Fact]
        public void DischargeIsInitializedCorrectly()
        {
            _testClass.Discharge.Should().Be(_discharge);
        }

        [Fact]
        public void CanSetAndGetDischarge()
        {
            // Arrange
            var testValue = 130176389.97;

            // Act
            _testClass.Discharge = testValue;

            // Assert
            _testClass.Discharge.Should().Be(testValue);
        }

        [Fact]
        public void BatteryPercentageIsInitializedCorrectly()
        {
            _testClass.BatteryPercentage.Should().Be(_batteryPercentage);
        }

        [Fact]
        public void CanSetAndGetBatteryPercentage()
        {
            // Arrange
            var testValue = 1248679308.69;

            // Act
            _testClass.BatteryPercentage = testValue;

            // Assert
            _testClass.BatteryPercentage.Should().Be(testValue);
        }
    }
}