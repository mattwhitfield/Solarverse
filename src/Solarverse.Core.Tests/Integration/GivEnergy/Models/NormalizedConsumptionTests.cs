namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class NormalizedConsumptionTests
    {
        private NormalizedConsumption _testClass;
        private ConsumptionHistory _history;

        public NormalizedConsumptionTests()
        {
            _history = new ConsumptionHistory { DataPoints = new List<ConsumptionDataPoint>() };
            _testClass = new NormalizedConsumption(_history);

            // TODO - proper tests
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new NormalizedConsumption(_history);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CanGetContainsInterpolatedPoints()
        {
            // Assert
            _testClass.ContainsInterpolatedPoints.As<object>().Should().BeAssignableTo<bool>();
        }

        [Fact]
        public void CanGetIsValid()
        {
            // Assert
            _testClass.IsValid.As<object>().Should().BeAssignableTo<bool>();
        }

        [Fact]
        public void CanGetDataPoints()
        {
            // Assert
            _testClass.DataPoints.Should().BeAssignableTo<List<NormalizedConsumptionDataPoint>>();
        }
    }
}