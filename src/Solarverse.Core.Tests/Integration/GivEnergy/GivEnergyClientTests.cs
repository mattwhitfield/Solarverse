namespace Solarverse.Core.Tests.Integration.GivEnergy
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Data;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration.GivEnergy;
    using Xunit;

    public class GivEnergyClientTests
    {
        private GivEnergyClient _testClass;
        private ILogger<GivEnergyClient> _logger;
        private IConfigurationProvider _configurationProvider;
        private ICurrentDataService _currentDataService;
        private ICurrentTimeProvider _currentTimeProvider;

        public GivEnergyClientTests()
        {
            _logger = Substitute.For<ILogger<GivEnergyClient>>();
            _configurationProvider = Substitute.For<IConfigurationProvider>();
            _configurationProvider.Configuration.Returns(StaticConfigurationProvider.Configuration);
            _currentDataService = Substitute.For<ICurrentDataService>();
            _currentTimeProvider = new CurrentTimeProvider(_configurationProvider);
            _testClass = new GivEnergyClient(_logger, _configurationProvider, _currentDataService, _currentTimeProvider);
        }


        [Fact]
        public async Task CanCallGetData()
        {
            // Act
            var result = await _testClass.GetHouseholdConsumptionFor(new DateTime(2024, 3, 31));

            // Assert
            Console.WriteLine(result);
        }

        [Fact]
        public async Task CanCallFindEVChargerUuid()
        {
            // Act
            var result = await _testClass.FindEVChargerUuid();

            // Assert
            Console.WriteLine(result);
        }

        [Fact]
        public async Task CanCallSetChargingEnabled()
        {
            // Arrange
            var enabled = true;

            // Act
            await _testClass.SetChargingEnabled(enabled);

            // Assert

        }
    }
}