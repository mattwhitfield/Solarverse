namespace Solarverse.Core.Tests.Integration.GivEnergy
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration.GivEnergy;
    using Xunit;

    public class GivEnergyClientTests
    {
        private GivEnergyClient _testClass;
        private string _key;

        public GivEnergyClientTests()
        {
            _key = ConfigurationProvider.Configuration.ApiKeys.GivEnergy;
            _testClass = new GivEnergyClient(_key);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new GivEnergyClient(_key);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public async Task CanCallFindInverterSerial()
        {
            // Act
            var result = await _testClass.FindInverterSerial();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallGetCurrentState()
        {
            // Act
            var result = await _testClass.GetCurrentState();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallGetAllSettings()
        {
            // Act
            var result = await _testClass.GetAllSettings();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallReadSetting()
        {
            // Arrange
            var id = await _testClass.GetSettingId("AC Charge 1 Start Time");

            // Act
            var result = await _testClass.ReadSetting(id);

            // Assert
        }

        [Fact]
        public async Task CanCallSetSetting()
        {
            // Arrange
            var id = await _testClass.GetSettingId("AC Charge 1 Start Time");
            var value = "01:35";

            // Act
            var result = await _testClass.SetSetting(id, value);

            // Assert
        }
    }
}