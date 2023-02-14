namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class CurrentSettingValuesTests
    {
        private CurrentSettingValues _testClass;
        private BoolSetting _ecoModeEnabled;
        private BatteryModeSettingValues _chargeSettings;
        private BatteryModeSettingValues _dischargeSettings;

        public CurrentSettingValuesTests()
        {
            _ecoModeEnabled = new BoolSetting(1935547577, true);
            _chargeSettings = new BatteryModeSettingValues(new TimeSetting(69410094, TimeSpan.FromSeconds(248)), new TimeSetting(2086586352, TimeSpan.FromSeconds(435)), new BoolSetting(1226366605, true), new IntSetting(1845863187, 1412571734));
            _dischargeSettings = new BatteryModeSettingValues(new TimeSetting(1315911299, TimeSpan.FromSeconds(393)), new TimeSetting(1981578600, TimeSpan.FromSeconds(385)), new BoolSetting(461911620, true), new IntSetting(1037891159, 597187882));
            _testClass = new CurrentSettingValues(_ecoModeEnabled, _chargeSettings, _dischargeSettings);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new CurrentSettingValues(_ecoModeEnabled, _chargeSettings, _dischargeSettings);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void EcoModeEnabledIsInitializedCorrectly()
        {
            _testClass.EcoModeEnabled.Should().BeSameAs(_ecoModeEnabled);
        }

        [Fact]
        public void ChargeSettingsIsInitializedCorrectly()
        {
            _testClass.ChargeSettings.Should().BeSameAs(_chargeSettings);
        }

        [Fact]
        public void DischargeSettingsIsInitializedCorrectly()
        {
            _testClass.DischargeSettings.Should().BeSameAs(_dischargeSettings);
        }
    }
}