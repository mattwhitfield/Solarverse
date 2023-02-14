namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class BoolSettingTests
    {
        private BoolSetting _testClass;
        private int _id;
        private bool _value;

        public BoolSettingTests()
        {
            _id = 1125494679;
            _value = true;
            _testClass = new BoolSetting(_id, _value);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new BoolSetting(_id, _value);

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