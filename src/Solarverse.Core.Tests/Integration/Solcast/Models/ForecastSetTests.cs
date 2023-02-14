namespace Solarverse.Core.Tests.Integration.Solcast.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.Solcast.Models;
    using Xunit;

    public class ForecastSetTests
    {
        private ForecastSet _testClass;

        public ForecastSetTests()
        {
            _testClass = new ForecastSet();
        }

        [Fact]
        public void CanSetAndGetForecasts()
        {
            // Arrange
            var testValue = new List<Forecast>();

            // Act
            _testClass.Forecasts = testValue;

            // Assert
            _testClass.Forecasts.Should().BeSameAs(testValue);
        }
    }
}