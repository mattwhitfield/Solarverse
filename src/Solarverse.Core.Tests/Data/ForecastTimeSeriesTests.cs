namespace Solarverse.Core.Tests.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Data;
    using Solarverse.Core.Helper;
    using Xunit;

    public class ForecastTimeSeriesTests
    {
        private ForecastTimeSeries _testClass;
        private IEnumerable<TimeSeriesPoint> _points;
        private ILogger _logger;
        private ICurrentDataService _currentDataService;
        private IConfigurationProvider _configurationProvider;

        public ForecastTimeSeriesTests()
        {
            _points = new[] { new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow) };
            _logger = Substitute.For<ILogger>();
            _currentDataService = Substitute.For<ICurrentDataService>();
            _configurationProvider = Substitute.For<IConfigurationProvider>();
            _testClass = new ForecastTimeSeries(_points, _logger, _currentDataService, _configurationProvider);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ForecastTimeSeries(_points, _logger, _currentDataService, _configurationProvider);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullPoints()
        {
            FluentActions.Invoking(() => new ForecastTimeSeries(default(IEnumerable<TimeSeriesPoint>), Substitute.For<ILogger>(), Substitute.For<ICurrentDataService>(), Substitute.For<IConfigurationProvider>())).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullLogger()
        {
            FluentActions.Invoking(() => new ForecastTimeSeries(new[] { new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow) }, default(ILogger), Substitute.For<ICurrentDataService>(), Substitute.For<IConfigurationProvider>())).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullCurrentDataService()
        {
            FluentActions.Invoking(() => new ForecastTimeSeries(new[] { new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow) }, Substitute.For<ILogger>(), default(ICurrentDataService), Substitute.For<IConfigurationProvider>())).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullConfigurationProvider()
        {
            FluentActions.Invoking(() => new ForecastTimeSeries(new[] { new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow) }, Substitute.For<ILogger>(), Substitute.For<ICurrentDataService>(), default(IConfigurationProvider))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ImplementsIEnumerable_TimeSeriesPoint()
        {
            // Arrange
            ForecastTimeSeries enumerable = default(ForecastTimeSeries);
            int expectedCount = -1;
            int actualCount = 0;

            // Act
            using (var enumerator = enumerable.GetEnumerator())
            {
                enumerator.Should().NotBeNull();
                while (enumerator.MoveNext())
                {
                    actualCount++;
                    enumerator.Current.Should().BeAssignableTo<TimeSeriesPoint>();
                }
            }

            // Assert
            actualCount.Should().Be(expectedCount);
        }

        [Fact]
        public void CanCallGetEnumeratorWithNoParameters()
        {
            // Act
            var result = _testClass.GetEnumerator();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallRunActionOnDischargeStartPeriods()
        {
            // Arrange
            var passName = "TestValue320130401";
            Action<(TimeSeriesPoint Point, double PointPercentRequired, IList<TimeSeriesPoint> DischargePoints)> action = x => { };

            // Act
            _testClass.RunActionOnDischargeStartPeriods(passName, action);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallRunActionOnDischargeStartPeriodsWithNullAction()
        {
            FluentActions.Invoking(() => _testClass.RunActionOnDischargeStartPeriods("TestValue1723077143", default(Action<(TimeSeriesPoint Point, double PointPercentRequired, IList<TimeSeriesPoint> DischargePoints)>))).Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CannotCallRunActionOnDischargeStartPeriodsWithInvalidPassName(string value)
        {
            FluentActions.Invoking(() => _testClass.RunActionOnDischargeStartPeriods(value, x => { })).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallGetEnumeratorForIEnumerableWithNoParameters()
        {
            // Act
            var result = ((IEnumerable)_testClass).GetEnumerator();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetEfficiency()
        {
            // Assert
            _testClass.Efficiency.As<object>().Should().BeAssignableTo<double>();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetMaxChargeKwhPerPeriod()
        {
            // Assert
            _testClass.MaxChargeKwhPerPeriod.As<object>().Should().BeAssignableTo<double>();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetCapacity()
        {
            // Assert
            _testClass.Capacity.As<object>().Should().BeAssignableTo<double>();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetReserve()
        {
            // Assert
            _testClass.Reserve.As<object>().Should().BeAssignableTo<int>();

            throw new NotImplementedException("Create or modify test");
        }
    }
}