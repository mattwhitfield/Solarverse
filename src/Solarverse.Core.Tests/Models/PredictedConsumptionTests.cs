namespace Solarverse.Core.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class PredictedConsumptionTests
    {
        private PredictedConsumption _testClass;
        private IEnumerable<PredictedConsumption> _sourcesIEnumerablePredictedConsumption;
        private IEnumerable<HouseholdConsumption> _sourcesIEnumerableHouseholdConsumption;
        private DateTime _date;

        public PredictedConsumptionTests()
        {
            _sourcesIEnumerablePredictedConsumption = new[] { new PredictedConsumption(default(IEnumerable<PredictedConsumption>)), new PredictedConsumption(default(IEnumerable<PredictedConsumption>)), new PredictedConsumption(default(IEnumerable<PredictedConsumption>)) };
            _sourcesIEnumerableHouseholdConsumption = new[] { new HouseholdConsumption(false, false, default(IEnumerable<HouseholdConsumptionDataPoint>)), new HouseholdConsumption(true, true, default(IEnumerable<HouseholdConsumptionDataPoint>)), new HouseholdConsumption(true, true, default(IEnumerable<HouseholdConsumptionDataPoint>)) };
            _date = DateTime.UtcNow;
            _testClass = new PredictedConsumption(_sourcesIEnumerableHouseholdConsumption, _date);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new PredictedConsumption(_sourcesIEnumerablePredictedConsumption);

            // Assert
            instance.Should().NotBeNull();

            // Act
            instance = new PredictedConsumption(_sourcesIEnumerableHouseholdConsumption, _date);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullSources()
        {
            FluentActions.Invoking(() => new PredictedConsumption(default(IEnumerable<PredictedConsumption>))).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new PredictedConsumption(default(IEnumerable<HouseholdConsumption>), DateTime.UtcNow)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallLimitTo()
        {
            // Arrange
            var @from = DateTime.UtcNow;
            var to = DateTime.UtcNow;

            // Act
            var result = _testClass.LimitTo(from, to);

            // Assert
            throw new NotImplementedException("Create or modify test");
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
            _testClass.DataPoints.Should().BeAssignableTo<IList<HouseholdConsumptionDataPoint>>();

            throw new NotImplementedException("Create or modify test");
        }
    }
}