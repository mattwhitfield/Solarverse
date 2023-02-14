namespace Solarverse.Core.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class InverterCurrentStateTests
    {
        private InverterCurrentState _testClass;
        private DateTime _updateTime;
        private int _currentSolarPower;
        private int _batteryPercent;
        private double _maxDischargeRateKw;
        private double _maxChargeRateKw;
        private int _batteryReserve;

        public InverterCurrentStateTests()
        {
            _updateTime = DateTime.UtcNow;
            _currentSolarPower = 633676559;
            _batteryPercent = 1302911875;
            _maxDischargeRateKw = 1939893789.24;
            _maxChargeRateKw = 1994122033.2;
            _batteryReserve = 2111437217;
            _testClass = new InverterCurrentState(_updateTime, _currentSolarPower, _batteryPercent, _maxDischargeRateKw, _maxChargeRateKw, _batteryReserve);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new InverterCurrentState(_updateTime, _currentSolarPower, _batteryPercent, _maxDischargeRateKw, _maxChargeRateKw, _batteryReserve);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CanGetDefault()
        {
            // Assert
            InverterCurrentState.Default.Should().BeAssignableTo<InverterCurrentState>();
            InverterCurrentState.Default.BatteryPercent.Should().Be(0);
            InverterCurrentState.Default.BatteryReserve.Should().Be(4);
            InverterCurrentState.Default.CurrentSolarPower.Should().Be(0);
            InverterCurrentState.Default.MaxChargeRateKw.Should().Be(2.6);
            InverterCurrentState.Default.MaxDischargeRateKw.Should().Be(2.6);
        }

        [Fact]
        public void UpdateTimeIsInitializedCorrectly()
        {
            _testClass.UpdateTime.Should().Be(_updateTime);
        }

        [Fact]
        public void CurrentSolarPowerIsInitializedCorrectly()
        {
            _testClass.CurrentSolarPower.Should().Be(_currentSolarPower);
        }

        [Fact]
        public void BatteryPercentIsInitializedCorrectly()
        {
            _testClass.BatteryPercent.Should().Be(_batteryPercent);
        }

        [Fact]
        public void MaxDischargeRateKwIsInitializedCorrectly()
        {
            _testClass.MaxDischargeRateKw.Should().Be(_maxDischargeRateKw);
        }

        [Fact]
        public void MaxChargeRateKwIsInitializedCorrectly()
        {
            _testClass.MaxChargeRateKw.Should().Be(_maxChargeRateKw);
        }

        [Fact]
        public void BatteryReserveIsInitializedCorrectly()
        {
            _testClass.BatteryReserve.Should().Be(_batteryReserve);
        }

        [Fact]
        public void CanGetExtendedProperties()
        {
            // Assert
            _testClass.ExtendedProperties.Should().BeAssignableTo<Dictionary<string, string>>();
        }
    }
}