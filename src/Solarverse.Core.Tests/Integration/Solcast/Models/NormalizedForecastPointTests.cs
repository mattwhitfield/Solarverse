namespace Solarverse.Core.Tests.Integration.Solcast.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.Solcast.Models;
    using Xunit;

    public class NormalizedForecastPointTests
    {
        private NormalizedForecastPoint _testClass;
        private DateTime _time;
        private double _pVEstimate;

        public NormalizedForecastPointTests()
        {
            _time = DateTime.UtcNow;
            _pVEstimate = 2047409399.97;
            _testClass = new NormalizedForecastPoint(_time, _pVEstimate);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new NormalizedForecastPoint(_time, _pVEstimate);

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