namespace Solarverse.Core.Tests.Data
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Xunit;

    public class RenderedNullableTimeSeriesPointTests
    {
        private RenderedNullableTimeSeriesPoint _testClass;
        private DateTime _time;
        private double? _value;

        public RenderedNullableTimeSeriesPointTests()
        {
            _time = DateTime.UtcNow;
            _value = 1377280006.74;
            _testClass = new RenderedNullableTimeSeriesPoint(_time, _value);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new RenderedNullableTimeSeriesPoint(_time, _value);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void TimeIsInitializedCorrectly()
        {
            _testClass.Time.Should().Be(_time);
        }

        [Fact]
        public void ValueIsInitializedCorrectly()
        {
            _testClass.Value.Should().Be(_value);
        }
    }
}