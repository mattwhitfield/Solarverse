namespace Solarverse.Core.Helper
{
    public static class TypeExtensions
    {
        public static string GetFormattedName(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments().Select(x => x.GetFormattedName()).Aggregate((x, y) => $"{x}, {y}");
                return string.Concat(type.Name.AsSpan(0, type.Name.IndexOf("`")), "<", genericArguments, ">");
            }
            return type.Name;
        }
    }
}
