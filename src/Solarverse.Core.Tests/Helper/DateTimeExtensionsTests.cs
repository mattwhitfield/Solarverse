namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;

    public static class DateTimeExtensionsTests
    {
        [Fact]
        public static void CanCallToHalfHourPeriod()
        {
            // Arrange
            var d = DateTime.UtcNow;

            // Act
            var result = d.ToHalfHourPeriod();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}