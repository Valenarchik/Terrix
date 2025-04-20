using System.Collections.Generic;

namespace CustomUtilities.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string JoinStrings(this IEnumerable<string> e) => e.JoinStrings(string.Empty);
        public static string JoinStrings(this IEnumerable<string> e, string separator) => string.Join(separator, e);
        public static string JoinStrings<T>(this IEnumerable<T> e, string separator) => string.Join<T>(separator, e);
        public static string JoinStrings<T>(this IEnumerable<T> e) => e.JoinStrings<T>(string.Empty);

        public static string ArrayToString<T>(this T[] array)
        {
            if (array == null)
            {
                return null;
            }

            return $"[{array.JoinStrings(", ")}]";
        }
    }
}