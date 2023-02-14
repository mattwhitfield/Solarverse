namespace Solarverse.Core.Tests.Integration.Octopus.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.Octopus.Models;
    using Xunit;

    public class TariffTypeTests
    {
        private TariffType _testClass;

        public TariffTypeTests()
        {
            _testClass = new TariffType();
        }

        [Fact]
        public void CanSetAndGetDirectDebitMonthly()
        {
            // Arrange
            var testValue = new Tariff { Code = "TestValue1826673025" };

            // Act
            _testClass.DirectDebitMonthly = testValue;

            // Assert
            _testClass.DirectDebitMonthly.Should().BeSameAs(testValue);
        }
    }
}