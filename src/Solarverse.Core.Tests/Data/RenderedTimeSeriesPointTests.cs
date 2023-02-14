namespace Solarverse.Core.Tests.Data
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Xunit;

    public class RenderedTimeSeriesPointTests
    {
        private RenderedTimeSeriesPoint _testClass;
        private DateTime _time;
        private double _value;

        public RenderedTimeSeriesPointTests()
        {
            _time = DateTime.UtcNow;
            _value = 963225635.13;
            _testClass = new RenderedTimeSeriesPoint(_time, _value);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new RenderedTimeSeriesPoint(_time, _value);

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