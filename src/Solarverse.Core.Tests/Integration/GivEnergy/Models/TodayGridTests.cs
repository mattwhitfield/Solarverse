namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class TodayGridTests
    {
        private TodayGrid _testClass;

        public TodayGridTests()
        {
            _testClass = new TodayGrid();
        }

        [Fact]
        public void CanSetAndGetImport()
        {
            // Arrange
            var testValue = 437731311.6;

            // Act
            _testClass.Import = testValue;

            // Assert
            _testClass.Import.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetExport()
        {
            // Arrange
            var testValue = 1234359985.32;

            // Act
            _testClass.Export = testValue;

            // Assert
            _testClass.Export.Should().Be(testValue);
        }
    }
}