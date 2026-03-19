/*
Compatibility extension methods for char.ToUpperInvariant() / ToLowerInvariant()
used as instance methods in the shared formatter library code.
*/

namespace PoorMansTSqlFormatterLib
{
    internal static class CharExtensions
    {
        public static char ToLowerInvariant(this char value) => char.ToLowerInvariant(value);
        public static char ToUpperInvariant(this char value) => char.ToUpperInvariant(value);
    }
}
