namespace Solarverse.Core.Tests.Helper
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;

    public static class TypeExtensionsTests
    {
        [Fact]
        public static void CanCallGetFormattedName()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.GetFormattedName();

            // Assert
            result.Should().Be("String");
        }

        [Fact]
        public static void CanCallGetFormattedNameWithGeneric()
        {
            // Arrange
            var type = typeof(IEnumerable<string>);

            // Act
            var result = type.GetFormattedName();

            // Assert
            result.Should().Be("IEnumerable<String>");
        }

        [Fact]
        public static void CanCallGetFormattedNameWithDoubleGeneric()
        {
            // Arrange
            var type = typeof(IEnumerable<IEnumerable<string>>);

            // Act
            var result = type.GetFormattedName();

            // Assert
            result.Should().Be("IEnumerable<IEnumerable<String>>");
        }
    }
}