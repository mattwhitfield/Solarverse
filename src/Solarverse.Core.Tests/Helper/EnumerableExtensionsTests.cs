namespace Solarverse.Core.Tests.Helper
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;
    using T = System.String;

    public static class EnumerableExtensionsTests
    {
        [Fact]
        public static void CanCallEach()
        {
            // Arrange
            var list = new List<string>();
            var source = new[] { "TestValue464913635", "TestValue2092296880", "TestValue1567165296" };
            Action<T> action = list.Add;

            // Act
            source.Each(action);

            // Assert
            list.Should().BeEquivalentTo(source);
        }
    }
}