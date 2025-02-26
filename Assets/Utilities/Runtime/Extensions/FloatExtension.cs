using System.Linq;

namespace CustomUtilities.Extensions
{
    public static class FloatExtension
    {
        public static string ToChars(this float d, int charsAmount)
        {
            var str = ((int) d).ToString();
            if (str.Length >= charsAmount)
            {
                return str;
            }

            return new string('0', charsAmount - str.Length) + str;
        }
    }
}
