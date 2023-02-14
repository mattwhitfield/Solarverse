namespace Solarverse.Core.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Data;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Models;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class CurrentDataServiceTests
    {
        private CurrentDataService _testClass;
        private ILogger<CurrentDataService> _logger;
        private IConfigurationProvider _configurationProvider;

        public CurrentDataServiceTests()
        {
            _logger = Substitute.For<ILogger<CurrentDataService>>();
            _configurationProvider = Substitute.For<IConfigurationProvider>();
            _testClass = new CurrentDataService(_logger, _configurationProvider);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new CurrentDataService(_logger, _configurationProvider);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullLogger()
        {
            FluentActions.Invoking(() => new CurrentDataService(default(ILogger<CurrentDataService>), Substitute.For<IConfigurationProvider>())).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullConfigurationProvider()
        {
            FluentActions.Invoking(() => new CurrentDataService(Substitute.For<ILogger<CurrentDataService>>(), default(IConfigurationProvider))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallGetForecastTimeSeries()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();

            // Act
            var result = _testClass.GetForecastTimeSeries(logger);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallGetForecastTimeSeriesWithNullLogger()
        {
            FluentActions.Invoking(() => _testClass.GetForecastTimeSeries(default(ILogger))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallCull()
        {
            // Arrange
            var deleteOlderThan = TimeSpan.FromSeconds(138);

            // Act
            _testClass.Cull(deleteOlderThan);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallLockForUpdate()
        {
            // Act
            var result = _testClass.LockForUpdate();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallUpdateWithCurrentState()
        {
            // Arrange
            var currentState = new InverterCurrentState(DateTime.UtcNow, 1576818177, 231054453, 1699307645.31, 2061945265.05, 823606209);

            // Act
            _testClass.Update(currentState);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateWithCurrentStateWithNullCurrentState()
        {
            FluentActions.Invoking(() => _testClass.Update(default(InverterCurrentState))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallUpdateWithPredictedConsumption()
        {
            // Arrange
            var consumption = new PredictedConsumption(new PredictedConsumption[0]);

            // Act
            _testClass.Update(consumption);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateWithPredictedConsumptionWithNullConsumption()
        {
            FluentActions.Invoking(() => _testClass.Update(default(PredictedConsumption))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallUpdateWithHouseholdConsumption()
        {
            // Arrange
            var consumption = new HouseholdConsumption(true, false, new[] { new HouseholdConsumptionDataPoint(DateTime.UtcNow, 790206438.78, 1638497825.91, 964958556.87, 254067360.03, 1021285035.54, 506430297.45, 603334235.79), new HouseholdConsumptionDataPoint(DateTime.UtcNow, 609570124.02, 130725256.86, 1874630257.83, 967833910.89, 2098392789.24, 329488662.69, 731308648.95), new HouseholdConsumptionDataPoint(DateTime.UtcNow, 39531489.03, 1731276917.37, 1636608285.18, 1246593405.42, 613042626.24, 1905707966.58, 1286159241.51) });

            // Act
            _testClass.Update(consumption);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateWithHouseholdConsumptionWithNullConsumption()
        {
            FluentActions.Invoking(() => _testClass.Update(default(HouseholdConsumption))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallUpdateWithForecast()
        {
            // Arrange
            var forecast = new SolarForecast(false, new[] { new SolarForecastPoint(DateTime.UtcNow, 679887448.02), new SolarForecastPoint(DateTime.UtcNow, 399522167.55), new SolarForecastPoint(DateTime.UtcNow, 1769734904.85) });

            // Act
            _testClass.Update(forecast);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateWithForecastWithNullForecast()
        {
            FluentActions.Invoking(() => _testClass.Update(default(SolarForecast))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallUpdateIncomingRates()
        {
            // Arrange
            var incomingRates = new[] { new TariffRate(1028388771.63, DateTime.UtcNow, DateTime.UtcNow), new TariffRate(2089081351.17, DateTime.UtcNow, DateTime.UtcNow), new TariffRate(1590798091.41, DateTime.UtcNow, DateTime.UtcNow) };

            // Act
            _testClass.UpdateIncomingRates(incomingRates);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateIncomingRatesWithNullIncomingRates()
        {
            FluentActions.Invoking(() => _testClass.UpdateIncomingRates(default(IList<TariffRate>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallUpdateOutgoingRates()
        {
            // Arrange
            var outgoingRates = new[] { new TariffRate(1399855125.24, DateTime.UtcNow, DateTime.UtcNow), new TariffRate(1251305410.41, DateTime.UtcNow, DateTime.UtcNow), new TariffRate(1843609085.51, DateTime.UtcNow, DateTime.UtcNow) };

            // Act
            _testClass.UpdateOutgoingRates(outgoingRates);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateOutgoingRatesWithNullOutgoingRates()
        {
            FluentActions.Invoking(() => _testClass.UpdateOutgoingRates(default(IList<TariffRate>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallRecalculateForecast()
        {
            // Arrange
            _configurationProvider.Configuration.Returns(new Configuration { SolcastSiteId = "TestValue1114417445", TestMode = false, ApiKey = new Guid("7ffaabe9-9ec9-4232-ad15-d58ce92401df") });

            // Act
            _testClass.RecalculateForecast();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallUpdateCurrentState()
        {
            // Arrange
            Action<InverterCurrentState> updateAction = x => { };

            // Act
            _testClass.UpdateCurrentState(updateAction);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallUpdateCurrentStateWithNullUpdateAction()
        {
            FluentActions.Invoking(() => _testClass.UpdateCurrentState(default(Action<InverterCurrentState>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanGetTimeSeries()
        {
            // Assert
            _testClass.TimeSeries.Should().BeAssignableTo<TimeSeries>();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetCurrentState()
        {
            // Assert
            _testClass.CurrentState.Should().BeAssignableTo<InverterCurrentState>();

            throw new NotImplementedException("Create or modify test");
        }
    }
}