namespace Solarverse.Core.Tests.Data
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Data;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration;
    using Xunit;

    public class FileDataStoreTests
    {
        private FileDataStore _testClass;
        private IIntegrationProvider _integrationProvider;
        private ICachePathProvider _cachePathProvider;
        private ILogger<FileDataStore> _logger;

        public FileDataStoreTests()
        {
            _integrationProvider = Substitute.For<IIntegrationProvider>();
            _cachePathProvider = Substitute.For<ICachePathProvider>();
            _logger = Substitute.For<ILogger<FileDataStore>>();
            _testClass = new FileDataStore(_integrationProvider, _cachePathProvider, _logger, Substitute.For<ICurrentTimeProvider>());
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new FileDataStore(_integrationProvider, _cachePathProvider, _logger, Substitute.For<ICurrentTimeProvider>());

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public async Task CanCallGetHouseholdConsumptionFor()
        {
            // Arrange
            var date = DateTime.UtcNow;

            _integrationProvider.InverterClient.Returns(Substitute.For<IInverterClient>());

            // Act
            var result = await _testClass.GetHouseholdConsumptionFor(date);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallGetSolarForecast()
        {
            // Arrange
            _integrationProvider.SolarForecastClient.Returns(Substitute.For<ISolarForecastClient>());

            // Act
            var result = await _testClass.GetSolarForecast();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallGetTariffRates()
        {
            // Arrange
            var productCode = "TestValue595340297";
            var mpan = "TestValue538672675";

            _integrationProvider.EnergySupplierClient.Returns(Substitute.For<IEnergySupplierClient>());

            // Act
            var result = await _testClass.GetTariffRates(productCode, mpan);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}