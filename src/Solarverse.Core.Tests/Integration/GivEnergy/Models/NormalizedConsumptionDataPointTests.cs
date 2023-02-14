namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class NormalizedConsumptionDataPointTests
    {
        private NormalizedConsumptionDataPoint _testClass;
        private DateTime _time;
        private double _consumption;
        private double _solar;
        private double _import;
        private double _export;
        private double _charge;
        private double _discharge;
        private double _batteryPercentage;

        public NormalizedConsumptionDataPointTests()
        {
            _time = DateTime.UtcNow;
            _consumption = 816064536.87;
            _solar = 2112841093.77;
            _import = 1608097895.91;
            _export = 366207616.17;
            _charge = 1437151397.22;
            _discharge = 2048169566.52;
            _batteryPercentage = 1712083928.94;
            _testClass = new NormalizedConsumptionDataPoint(_time, _consumption, _solar, _import, _export, _charge, _discharge, _batteryPercentage);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new NormalizedConsumptionDataPoint(_time, _consumption, _solar, _import, _export, _charge, _discharge, _batteryPercentage);

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
        public void SolarIsInitializedCorrectly()
        {
            _testClass.Solar.Should().Be(_solar);
        }

        [Fact]
        public void ImportIsInitializedCorrectly()
        {
            _testClass.Import.Should().Be(_import);
        }

        [Fact]
        public void ExportIsInitializedCorrectly()
        {
            _testClass.Export.Should().Be(_export);
        }

        [Fact]
        public void ChargeIsInitializedCorrectly()
        {
            _testClass.Charge.Should().Be(_charge);
        }

        [Fact]
        public void DischargeIsInitializedCorrectly()
        {
            _testClass.Discharge.Should().Be(_discharge);
        }

        [Fact]
        public void BatteryPercentageIsInitializedCorrectly()
        {
            _testClass.BatteryPercentage.Should().Be(_batteryPercentage);
        }
    }
}