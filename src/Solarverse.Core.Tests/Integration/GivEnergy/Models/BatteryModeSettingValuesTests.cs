namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class BatteryModeSettingValuesTests
    {
        private BatteryModeSettingValues _testClass;
        private TimeSetting _startTime;
        private TimeSetting _endTime;
        private BoolSetting _enabled;
        private IntSetting _powerLimit;

        public BatteryModeSettingValuesTests()
        {
            _startTime = new TimeSetting(1902822439, TimeSpan.FromSeconds(254));
            _endTime = new TimeSetting(614408139, TimeSpan.FromSeconds(355));
            _enabled = new BoolSetting(287375528, true);
            _powerLimit = new IntSetting(21812159, 1815901523);
            _testClass = new BatteryModeSettingValues(_startTime, _endTime, _enabled, _powerLimit);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new BatteryModeSettingValues(_startTime, _endTime, _enabled, _powerLimit);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void StartTimeIsInitializedCorrectly()
        {
            _testClass.StartTime.Should().BeSameAs(_startTime);
        }

        [Fact]
        public void EndTimeIsInitializedCorrectly()
        {
            _testClass.EndTime.Should().BeSameAs(_endTime);
        }

        [Fact]
        public void EnabledIsInitializedCorrectly()
        {
            _testClass.Enabled.Should().BeSameAs(_enabled);
        }

        [Fact]
        public void PowerLimitIsInitializedCorrectly()
        {
            _testClass.PowerLimit.Should().BeSameAs(_powerLimit);
        }
    }
}