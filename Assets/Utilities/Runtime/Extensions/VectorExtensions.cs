using UnityEngine;

namespace CustomUtilities.Extensions
{
    public static class VectorExtensions
    {
        // Vector3
        public static Vector3 WithX(this Vector3 v3, float newX)
        {
            return new Vector3(newX, v3.y, v3.z);
        }

        public static Vector3 WithY(this Vector3 v3, float newY)
        {
            return new Vector3(v3.x, newY, v3.z);
        }

        public static Vector3 WithZ(this Vector3 v3, float newZ)
        {
            return new Vector3(v3.x, v3.y, newZ);
        }

        public static Vector3 SetX(this Vector3 v3, float newX)
        {
            v3.x = newX;
            return v3;
        }

        public static Vector3 SetY(this Vector3 v3, float newY)
        {
            v3.y = newY;
            return v3;
        }

        public static Vector3 SetZ(this Vector3 v3, float newZ)
        {
            v3.z = newZ;
            return v3;
        }

        // Vector 2
        public static Vector2 WithX(this Vector2 v2, float newX)
        {
            return new Vector2(newX, v2.y);
        }

        public static Vector2 WithY(this Vector2 v2, float newY)
        {
            return new Vector3(v2.x, newY);
        }

        public static Vector2 SetX(this Vector2 v2, float newX)
        {
            v2.x = newX;
            return v2;
        }

        public static Vector2 SetY(this Vector2 v2, float newY)
        {
            v2.y = newY;
            return v2;
        }
    }
}