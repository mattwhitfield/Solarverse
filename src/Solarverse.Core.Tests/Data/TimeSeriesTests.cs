namespace Solarverse.Core.Tests.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using FluentAssertions;
    using NSubstitute;
    using Solarverse.Core.Data;
    using Solarverse.Core.Helper;
    using Xunit;
    using TSource = System.String;
    using TValue = System.String;

    public class TimeSeriesTests
    {
        private TimeSeries _testClass;
        private IEnumerable<TimeSeriesPoint> _data;

        public TimeSeriesTests()
        {
            _data = new[] { new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow), new TimeSeriesPoint(DateTime.UtcNow) };
            _testClass = new TimeSeries(_data);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TimeSeries();

            // Assert
            instance.Should().NotBeNull();

            // Act
            instance = new TimeSeries(_data);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullData()
        {
            FluentActions.Invoking(() => new TimeSeries(default(IEnumerable<TimeSeriesPoint>))).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ImplementsIEnumerable_TimeSeriesPoint()
        {
            // Arrange
            TimeSeries enumerable = default(TimeSeries);
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
        public void CanCallAddPointsFrom()
        {
            // Arrange
            var points = new[] { "TestValue534215641", "TestValue967984287", "TestValue968293311" };
            Func<TSource, DateTime> dateTime = x => DateTime.UtcNow;
            Func<TSource, TValue> value = x => "TestValue571147868";
            Action<TValue, TimeSeriesPoint> @set = (x, y) => { };

            // Act
            _testClass.AddPointsFrom<TSource, TValue>(points, dateTime, value, set);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetNullableSeries()
        {
            // Arrange
            Func<TimeSeriesPoint, double?> value = x => 1197990969.12;

            // Act
            var result = _testClass.GetNullableSeries(value);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetSeries()
        {
            // Arrange
            Func<TimeSeriesPoint, double?> value = x => 681792997.05;

            // Act
            var result = _testClass.GetSeries(value);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetControlActions()
        {
            // Act
            var result = _testClass.GetControlActions();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetDates()
        {
            // Act
            var result = _testClass.GetDates();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallCull()
        {
            // Arrange
            var deleteOlderThan = TimeSpan.FromSeconds(345);

            // Act
            var result = _testClass.Cull(deleteOlderThan, Substitute.For<ICurrentTimeProvider>());

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallTryGetDataPointFor()
        {
            // Arrange
            var currentPeriod = DateTime.UtcNow;

            // Act
            var result = _testClass.TryGetDataPointFor(currentPeriod, out var currentDataPoint);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetMaximumDateWithNoParameters()
        {
            // Act
            var result = _testClass.GetMaximumDate();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetMaximumDateWithFuncOfTimeSeriesPointAndBool()
        {
            // Arrange
            Func<TimeSeriesPoint, bool> predicate = x => true;

            // Act
            var result = _testClass.GetMaximumDate(predicate);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetMinimumDateWithFuncOfTimeSeriesPointAndBool()
        {
            // Arrange
            Func<TimeSeriesPoint, bool> predicate = x => true;

            // Act
            var result = _testClass.GetMinimumDate(predicate);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetMinimumDateWithNoParameters()
        {
            // Act
            var result = _testClass.GetMinimumDate();

            // Assert
            throw new NotImplementedException("Create or modify test");
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
        public void CanCallGetEnumeratorForIEnumerableWithNoParameters()
        {
            // Act
            var result = ((IEnumerable)_testClass).GetEnumerator();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}