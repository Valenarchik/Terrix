using UnityEngine;

namespace CustomUtilities.Extensions
{
    public static class RectExtensions
    {
        public static bool AreIntersect(this Rect r1, Rect r2, bool checkBounds)
        {
            return CustomMath.SegmentsIntersect(r1.xMin, r1.xMax, r2.xMin, r2.xMax, checkBounds)
                   && CustomMath.SegmentsIntersect(r1.yMin, r1.yMax, r2.yMin, r2.yMax, checkBounds);
        }
    }
}