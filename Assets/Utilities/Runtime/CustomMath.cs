using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CustomUtilities
{
    public static class CustomMath
    {
        public static bool PointInCircle(Vector2Int checkedPoint, Vector2Int centerOfCircle, int radius)
        {
            var x = checkedPoint.x - centerOfCircle.x;
            var y = checkedPoint.y - centerOfCircle.y;
            return x * x + y * y <= radius * radius;
        }

        public static bool PointInCircle(Vector2 checkedPoint, Vector2 centerOfCircle, float radius)
        {
            return Mathf.Pow(checkedPoint.x - centerOfCircle.x, 2)
                + Mathf.Pow(checkedPoint.y - centerOfCircle.y, 2) <= Mathf.Pow(radius, 2);
        }

        public static bool SegmentsIntersect(Vector2 v11, Vector2 v12, Vector2 v21, Vector2 v22)
        {
            var cut1 = v12 - v11;
            var cut2 = v22 - v21;

            var prod1 = Vector3.Cross(cut1, v21 - v11);
            var prod2 = Vector3.Cross(cut1, v22 - v11);

            if (Math.Sign(prod1.z) == Math.Sign(prod2.z))
            {
                return false;
            }

            prod1 = Vector3.Cross(cut2, v11 - v21);
            prod2 = Vector3.Cross(cut2, v12 - v21);

            if (Math.Sign(prod1.z) == Math.Sign(prod2.z))
            {
                return false;
            }

            return true;
        }

        public static bool SegmentsIntersect(float a1, float a2, float b1, float b2, bool checkBounds)
        {
            var minMax = Math.Min(Math.Max(a1, a2), Math.Max(b1, b2));
            var maxMin = Math.Max(Math.Min(a1, a2), Math.Min(b1, b2));
            return checkBounds
                ? minMax >= maxMin
                : minMax > maxMin;
        }

        /// <summary>
        /// Пример (4,4) (2,1) -> (8,4)
        /// (4,4) (1,2) -> (4,8)
        /// (4,4) (12,24) -> (4,8)
        /// </summary>
        public static Vector2 TransferProportions(Vector2 vector2, Vector2 proportions)
        {
            if (proportions.x <= 0 || proportions.y <= 0)
            {
                throw new ArgumentException();
            }

            var proportionValue = proportions.x / proportions.y;
            if (proportionValue >= 1)
            {
                return new Vector2(proportionValue * vector2.x, vector2.y);
            }
            else
            {
                return new Vector2(vector2.x, vector2.y / proportionValue);
            }
        }

        public static Vector2Int TransferVector2ByMaxSide(Vector2Int vector2, int maxSide)
        {
            if (vector2.x <= 0 || vector2.y <= 0 || maxSide <= 0)
            {
                throw new ArgumentException();
            }

            if (vector2.x > vector2.y)
            {
                return new Vector2Int(maxSide, Convert.ToInt32(Convert.ToSingle(vector2.y * maxSide) / vector2.x));
            }
            else
            {
                return new Vector2Int(Convert.ToInt32(Convert.ToSingle(vector2.x * maxSide) / vector2.y), maxSide);
            }
        }

        public static int GreatestCommonDivisor(int m, int n)
        {
            if (m <= 0 || n <= 0)
            {
                throw new ArgumentException();
            }

            while (m != n)
            {
                if (m > n)
                {
                    m = m - n;
                }
                else
                {
                    n = n - m;
                }
            }

            return n;
        }

        public static float GetMinDistanceFromPointToSegment(Vector2 a, Vector2 b, Vector2 e)
        {
            var ab = b - a;
            var be = e - b;
            var ae = e - a;

            var abBe = (ab.x * be.x + ab.y * be.y);
            var abAe = (ab.x * ae.x + ab.y * ae.y);

            if (abBe > 0)
            {
                return be.magnitude;
            }

            if (abAe < 0)
            {
                return ae.magnitude;
            }

            return Mathf.Abs(ab.x * ae.y - ab.y * ae.x) / ab.magnitude;
        }

        public static Vector2 GetMinVectorFromSegmentToPoint(Vector2 a, Vector2 b, Vector2 e)
        {
            if (a == b)
            {
                return e - a;
            }

            var ab = b - a;
            var be = e - b;
            var ae = e - a;

            var abBe = (ab.x * be.x + ab.y * be.y);
            var abAe = (ab.x * ae.x + ab.y * ae.y);

            if (abBe > 0)
            {
                return be;
            }

            if (abAe < 0)
            {
                return ae;
            }

            var ortVector = new Vector2(-ab.y, ab.x);

            var a1x = ortVector.y;
            var a2x = -ortVector.x;
            var bx = ortVector.y * e.x - ortVector.x * e.y;

            var a1y = ab.y;
            var a2y = -ab.x;
            var by = ab.y * a.x - ab.x * a.y;

            var p = KramerSolution(new Vector2(a1x, a1y), new Vector2(a2x, a2y), new Vector2(bx, by));
            var pe = e - p.Value;
            return pe;
        }

        public static Vector2? KramerSolution(Vector2 xCoef, Vector2 yCoef, Vector2 freeCoef)
        {
            var determinant = xCoef.x * yCoef.y - xCoef.y * yCoef.x;
            if (determinant == 0)
            {
                return null;
            }

            var determinantX = freeCoef.x * yCoef.y - freeCoef.y * yCoef.x;
            var determinantY = xCoef.x * freeCoef.y - xCoef.y * freeCoef.x;
            return new Vector2(determinantX / determinant, determinantY / determinant);
        }
    }
}