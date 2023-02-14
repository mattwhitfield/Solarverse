namespace Solarverse.Core.Tests.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class TariffRateTests
    {
        private TariffRate _testClass;
        private double _value;
        private DateTime _validFrom;
        private DateTime _validTo;

        public TariffRateTests()
        {
            _value = 384751932.84;
            _validFrom = DateTime.UtcNow;
            _validTo = DateTime.UtcNow;
            _testClass = new TariffRate(_value, _validFrom, _validTo);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TariffRate(_value, _validFrom, _validTo);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void ValueIsInitializedCorrectly()
        {
            _testClass.Value.Should().Be(_value);
        }

        [Fact]
        public void ValidFromIsInitializedCorrectly()
        {
            _testClass.ValidFrom.Should().Be(_validFrom);
        }

        [Fact]
        public void ValidToIsInitializedCorrectly()
        {
            _testClass.ValidTo.Should().Be(_validTo);
        }
    }
}