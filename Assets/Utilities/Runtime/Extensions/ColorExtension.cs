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
    }
}