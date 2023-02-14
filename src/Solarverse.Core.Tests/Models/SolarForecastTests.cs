namespace Solarverse.Core.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class SolarForecastTests
    {
        private SolarForecast _testClass;
        private bool _isValid;
        private IEnumerable<SolarForecastPoint> _dataPoints;

        public SolarForecastTests()
        {
            _isValid = false;
            _dataPoints = new[] { new SolarForecastPoint(DateTime.UtcNow, 1157192302.86), new SolarForecastPoint(DateTime.UtcNow, 1393824878.82), new SolarForecastPoint(DateTime.UtcNow, 726180392.52) };
            _testClass = new SolarForecast(_isValid, _dataPoints);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new SolarForecast(_isValid, _dataPoints);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullDataPoints()
        {
            FluentActions.Invoking(() => new SolarForecast(false, default(IEnumerable<SolarForecastPoint>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsValidIsInitializedCorrectly()
        {
            _testClass.IsValid.Should().Be(_isValid);
        }

        [Fact]
        public void DataPointsIsInitializedCorrectly()
        {
            _testClass.DataPoints.Should().BeEquivalentTo(_dataPoints);
        }
    }
}