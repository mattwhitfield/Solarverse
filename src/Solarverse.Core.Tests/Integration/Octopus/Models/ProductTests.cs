namespace Solarverse.Core.Tests.Integration.Octopus.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.Octopus.Models;
    using Xunit;

    public class ProductTests
    {
        private Product _testClass;

        public ProductTests()
        {
            _testClass = new Product();
        }

        [Fact]
        public void CanSetAndGetCode()
        {
            // Arrange
            var testValue = "TestValue1957664662";

            // Act
            _testClass.Code = testValue;

            // Assert
            _testClass.Code.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetFullName()
        {
            // Arrange
            var testValue = "TestValue891058289";

            // Act
            _testClass.FullName = testValue;

            // Assert
            _testClass.FullName.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetTariffTypes()
        {
            // Arrange
            var testValue = new Dictionary<string, TariffType>();

            // Act
            _testClass.TariffTypes = testValue;

            // Assert
            _testClass.TariffTypes.Should().BeSameAs(testValue);
        }
    }
}