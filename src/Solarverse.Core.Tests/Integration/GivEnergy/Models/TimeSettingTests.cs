namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class TimeSettingTests
    {
        private TimeSetting _testClass;
        private int _id;
        private TimeSpan? _value;

        public TimeSettingTests()
        {
            _id = 1425512940;
            _value = TimeSpan.FromSeconds(244);
            _testClass = new TimeSetting(_id, _value);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TimeSetting(_id, _value);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void IdIsInitializedCorrectly()
        {
            _testClass.Id.Should().Be(_id);
        }

        [Fact]
        public void ValueIsInitializedCorrectly()
        {
            _testClass.Value.Should().Be(_value);
        }
    }
}