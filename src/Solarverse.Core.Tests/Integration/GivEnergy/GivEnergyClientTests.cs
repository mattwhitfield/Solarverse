namespace Solarverse.Core.Tests.Integration.GivEnergy
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
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
            _testClass = new GivEnergyClient(_configuration, Substitute.For<ILogger<GivEnergyClient>>());
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new GivEnergyClient(_configuration, Substitute.For<ILogger<GivEnergyClient>>());

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullConfiguration()
        {
            FluentActions.Invoking(() => new GivEnergyClient(default(Configuration), Substitute.For<ILogger<GivEnergyClient>>())).Should().Throw<ArgumentNullException>();
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
            // Act
            var result = await _testClass.SetSetting(SettingIds.Charge.EndTime, "23:00");

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