namespace Solarverse.Core.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class HouseholdConsumptionTests
    {
        private HouseholdConsumption _testClass;
        private bool _isValid;
        private bool _containsInterpolatedPoints;
        private IEnumerable<HouseholdConsumptionDataPoint> _dataPoints;

        public HouseholdConsumptionTests()
        {
            _isValid = true;
            _containsInterpolatedPoints = false;
            _dataPoints = new[] { new HouseholdConsumptionDataPoint(DateTime.UtcNow, 565147342.98, 1976092340.31, 1783539806.4, 117096163.47, 115683720.57, 119049535.44, 192047332.95), new HouseholdConsumptionDataPoint(DateTime.UtcNow, 308784602.61, 2005125309.99, 1262496904.02, 871841326.95, 194529302.55, 727855153.74, 793918302.21), new HouseholdConsumptionDataPoint(DateTime.UtcNow, 839876601.96, 177457860.35999998, 572312543.22, 139124192.13, 1798757700.3, 1598573631.6, 123928293.06) };
            _testClass = new HouseholdConsumption(_isValid, _containsInterpolatedPoints, _dataPoints);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new HouseholdConsumption(_isValid, _containsInterpolatedPoints, _dataPoints);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullDataPoints()
        {
            FluentActions.Invoking(() => new HouseholdConsumption(true, true, default(IEnumerable<HouseholdConsumptionDataPoint>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ContainsInterpolatedPointsIsInitializedCorrectly()
        {
            _testClass.ContainsInterpolatedPoints.Should().Be(_containsInterpolatedPoints);
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