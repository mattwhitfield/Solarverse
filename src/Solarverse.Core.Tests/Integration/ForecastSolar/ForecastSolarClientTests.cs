namespace Solarverse.Core.Tests.Integration.ForecastSolar
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration.ForecastSolar;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class ForecastSolarClientTests
    {
        private ForecastSolarClient _testClass;
        private IConfigurationProvider _configurationProvider;
        private ILogger<ForecastSolarClient> _logger;

        public ForecastSolarClientTests()
        {
            _configurationProvider = Substitute.For<IConfigurationProvider>();
            _logger = Substitute.For<ILogger<ForecastSolarClient>>();
            _testClass = new ForecastSolarClient(_configurationProvider, _logger);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ForecastSolarClient(_configurationProvider, _logger);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullConfigurationProvider()
        {
            FluentActions.Invoking(() => new ForecastSolarClient(default(IConfigurationProvider), Substitute.For<ILogger<ForecastSolarClient>>())).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullLogger()
        {
            FluentActions.Invoking(() => new ForecastSolarClient(Substitute.For<IConfigurationProvider>(), default(ILogger<ForecastSolarClient>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task CanCallGetForecast()
        {
            // Arrange
            _configurationProvider.Configuration.Returns(new Configuration { SolcastSiteId = "TestValue422992019", TestMode = true, ApiKey = new Guid("2900143c-7c6a-4336-9460-c6f39951c34d") });

            // Act
            var result = await _testClass.GetForecast();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}