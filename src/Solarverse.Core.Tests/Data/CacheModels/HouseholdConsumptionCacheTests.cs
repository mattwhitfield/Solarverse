namespace Solarverse.Core.Tests.Data.CacheModels
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Data.CacheModels;
    using Xunit;

    public class HouseholdConsumptionCacheTests
    {
        private HouseholdConsumptionCache _testClass;

        public HouseholdConsumptionCacheTests()
        {
            _testClass = new HouseholdConsumptionCache();
        }

        [Fact]
        public void CanSetAndGetContainsInterpolatedPoints()
        {
            // Arrange
            var testValue = true;

            // Act
            _testClass.ContainsInterpolatedPoints = testValue;

            // Assert
            _testClass.ContainsInterpolatedPoints.Should().Be(testValue);
        }

        [Fact]
        public void CanGetDataPoints()
        {
            // Assert
            _testClass.DataPoints.Should().BeAssignableTo<List<HouseholdConsumptionDataPointCache>>();
        }
    }
}