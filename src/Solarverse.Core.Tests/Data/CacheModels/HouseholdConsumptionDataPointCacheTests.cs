namespace Solarverse.Core.Tests.Data.CacheModels
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data.CacheModels;
    using Xunit;

    public class HouseholdConsumptionDataPointCacheTests
    {
        private HouseholdConsumptionDataPointCache _testClass;

        public HouseholdConsumptionDataPointCacheTests()
        {
            _testClass = new HouseholdConsumptionDataPointCache();
        }

        [Fact]
        public void CanSetAndGetTime()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.Time = testValue;

            // Assert
            _testClass.Time.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetConsumption()
        {
            // Arrange
            var testValue = 507372500.25;

            // Act
            _testClass.Consumption = testValue;

            // Assert
            _testClass.Consumption.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetSolar()
        {
            // Arrange
            var testValue = 1942278801.21;

            // Act
            _testClass.Solar = testValue;

            // Assert
            _testClass.Solar.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetImport()
        {
            // Arrange
            var testValue = 47281470.39;

            // Act
            _testClass.Import = testValue;

            // Assert
            _testClass.Import.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetExport()
        {
            // Arrange
            var testValue = 1449642045.06;

            // Act
            _testClass.Export = testValue;

            // Assert
            _testClass.Export.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetCharge()
        {
            // Arrange
            var testValue = 1637793209.25;

            // Act
            _testClass.Charge = testValue;

            // Assert
            _testClass.Charge.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetDischarge()
        {
            // Arrange
            var testValue = 508874944.05;

            // Act
            _testClass.Discharge = testValue;

            // Assert
            _testClass.Discharge.Should().Be(testValue);
        }

        [Fact]
        public void CanSetAndGetBatteryPercentage()
        {
            // Arrange
            var testValue = 1784688652.8899999;

            // Act
            _testClass.BatteryPercentage = testValue;

            // Assert
            _testClass.BatteryPercentage.Should().Be(testValue);
        }
    }
}