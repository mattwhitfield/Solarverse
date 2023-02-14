namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class ConsumptionHistoryTests
    {
        private ConsumptionHistory _testClass;

        public ConsumptionHistoryTests()
        {
            _testClass = new ConsumptionHistory();
        }

        [Fact]
        public void CanSetAndGetDataPoints()
        {
            // Arrange
            var testValue = new List<ConsumptionDataPoint>();

            // Act
            _testClass.DataPoints = testValue;

            // Assert
            _testClass.DataPoints.Should().BeSameAs(testValue);
        }
    }
}