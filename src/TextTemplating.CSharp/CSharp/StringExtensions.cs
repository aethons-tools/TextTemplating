namespace AethonsTool.TextTemplating.CSharp
{
    public static class StringExtensions
    {
        // TODO: check for keyword collisions and fix with "@"
        public static string ToParameterCase(this string @this) =>
            $"{char.ToLowerInvariant(@this[0])}{@this.Substring(1)}";
    }
}
