using UnityEngine;


namespace CustomUtilities.Extensions
{
    public static class ColorExtension
    {
        public static Color WithRed(this Color color, float newRed)
            => new(newRed, color.g, color.b, color.a);

        public static Color WithGreen(this Color color, float newGreen)
            => new(color.r, newGreen, color.b, color.a);

        public static Color WithBlue(this Color color, float newBlue)
            => new(color.r, color.g, newBlue, color.a);

        public static Color WithAlpha(this Color color, float newAlpha)
            => new(color.r, color.g, color.b, newAlpha);

        public static string ToHexString(this Color color)
            => ToHexString((Color32)color);

        public static string ToHexString(this Color32 c)
            => $"{c.r:X2}{c.g:X2}{c.b:X2}";

        public static float CalculateBrightness(this Color color)
        {
            return (0.299f * color.r + 0.587f * color.g + 0.114f * color.b) * color.a;
        }
    }
}