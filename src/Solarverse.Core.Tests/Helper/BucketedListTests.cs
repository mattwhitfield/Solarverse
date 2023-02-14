namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;

    public class BucketedListTests
    {
        private BucketedList _testClass;

        public BucketedListTests()
        {
            _testClass = new BucketedList();
        }

        [Fact]
        public void CanCallTakeSlot()
        {
            // Arrange
            var incomingRate = 1946647887.03;
            var efficiency = 1346258833.92;

            // Act
            var result = _testClass.TakeSlot(incomingRate, efficiency);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}