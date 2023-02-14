namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;

    public static class TypeExtensionsTests
    {
        [Fact]
        public static void CanCallGetFormattedName()
        {
            // Arrange
            var @type = typeof(string);

            // Act
            var result = type.GetFormattedName();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public static void CannotCallGetFormattedNameWithNullType()
        {
            FluentActions.Invoking(() => default(Type).GetFormattedName()).Should().Throw<ArgumentNullException>();
        }
    }
}