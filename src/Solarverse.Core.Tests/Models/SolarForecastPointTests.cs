namespace Solarverse.Core.Tests.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class SolarForecastPointTests
    {
        private SolarForecastPoint _testClass;
        private DateTime _time;
        private double _pVEstimate;

        public SolarForecastPointTests()
        {
            _time = DateTime.UtcNow;
            _pVEstimate = 1120746116.16;
            _testClass = new SolarForecastPoint(_time, _pVEstimate);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new SolarForecastPoint(_time, _pVEstimate);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void TimeIsInitializedCorrectly()
        {
            _testClass.Time.Should().Be(_time);
        }

        [Fact]
        public void PVEstimateIsInitializedCorrectly()
        {
            _testClass.PVEstimate.Should().Be(_pVEstimate);
        }
    }
}