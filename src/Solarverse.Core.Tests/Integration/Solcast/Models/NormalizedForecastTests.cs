namespace Solarverse.Core.Tests.Integration.Solcast.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.Solcast.Models;
    using Xunit;

    public class NormalizedForecastTests
    {
        private NormalizedForecast _testClass;
        private ForecastSet _forecast;

        public NormalizedForecastTests()
        {
            _forecast = new ForecastSet();
            _testClass = new NormalizedForecast(_forecast);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new NormalizedForecast(_forecast);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CanGetIsValid()
        {
            // Assert
            _testClass.IsValid.As<object>().Should().BeAssignableTo<bool>();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetDataPoints()
        {
            // Assert
            _testClass.DataPoints.Should().BeAssignableTo<List<NormalizedForecastPoint>>();

            throw new NotImplementedException("Create or modify test");
        }
    }
}