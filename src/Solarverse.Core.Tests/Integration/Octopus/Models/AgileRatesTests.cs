namespace Solarverse.Core.Tests.Integration.Octopus.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.Octopus.Models;
    using Xunit;

    public class AgileRatesTests
    {
        private AgileRates _testClass;

        public AgileRatesTests()
        {
            _testClass = new AgileRates();
        }

        [Fact]
        public void CanSetAndGetRates()
        {
            // Arrange
            var testValue = new List<AgileRate>();

            // Act
            _testClass.Rates = testValue;

            // Assert
            _testClass.Rates.Should().BeSameAs(testValue);
        }
    }
}