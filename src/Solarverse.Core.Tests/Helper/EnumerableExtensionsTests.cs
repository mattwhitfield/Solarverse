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
            var source = new[] { "TestValue464913635", "TestValue2092296880", "TestValue1567165296" };
            Action<T> action = x => { };

            // Act
            source.Each<T>(action);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallEachWithNullSource()
        {
            FluentActions.Invoking(() => default(IEnumerable<T>).Each<T>(x => { })).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public static void CannotCallEachWithNullAction()
        {
            FluentActions.Invoking(() => new[] { "TestValue1522136539", "TestValue1487988546", "TestValue1958657565" }.Each<T>(default(Action<T>))).Should().Throw<ArgumentNullException>();
        }
    }
}