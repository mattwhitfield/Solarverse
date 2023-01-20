namespace Solarverse.Core.Tests.Integration.GivEnergy
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration.GivEnergy;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class GivEnergyClientTests
    {
        private GivEnergyClient _testClass;
        private Configuration _configuration;

        public GivEnergyClientTests()
        {
            _configuration = ConfigurationProvider.Configuration;
            _testClass = new GivEnergyClient(_configuration);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new GivEnergyClient(_configuration);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullConfiguration()
        {
            FluentActions.Invoking(() => new GivEnergyClient(default(Configuration))).Should().Throw<ArgumentNullException>();
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
        }

        [Fact]
        public async Task CanCallReadSetting()
        {
            // Arrange
            var id = 1061037898;

            // Act
            var result = await _testClass.ReadSetting(id);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallSetSetting()
        {
            // Arrange
            var id = 1117878163;
            var value = new object();

            // Act
            var result = await _testClass.SetSetting(id, value);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallSetSettingWithNullValue()
        {
            await FluentActions.Invoking(() => _testClass.SetSetting(138087233, default(object))).Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CanCallGetHouseholdConsumptionFor()
        {
            // Arrange
            var date = DateTime.UtcNow;

            // Act
            var result = await _testClass.GetHouseholdConsumptionFor(date);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallCharge()
        {
            // Arrange
            var until = DateTime.UtcNow;

            // Act
            await _testClass.Charge(until);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallHold()
        {
            // Arrange
            var until = DateTime.UtcNow;

            // Act
            await _testClass.Hold(until);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallDischarge()
        {
            // Arrange
            var until = DateTime.UtcNow;

            // Act
            await _testClass.Discharge(until);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallExport()
        {
            // Arrange
            var until = DateTime.UtcNow;

            // Act
            await _testClass.Export(until);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}