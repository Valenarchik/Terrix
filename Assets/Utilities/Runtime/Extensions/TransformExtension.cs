using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUtilities.Extensions
{
    public static class TransformExtension
    {
        public static int GetLastActiveChildIndex(this Transform transform)
        {
            try
            {
                return transform.EnumerateChildren()
                    .Select((el, i) => (el, i))
                    .Last(tuple => tuple.el.gameObject.activeSelf)
                    .i;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        public static IEnumerable<Transform> EnumerateChildren(this Transform transform)
        {
            var childesCount = transform.childCount;
            for (int i = 0; i < childesCount; i++)
            {
                yield return transform.GetChild(i);
            }
        }

        public static IEnumerable<Transform> EnumerateAllChildren(this Transform parent)
        {
            var q = new Queue<Transform>();
            q.Enqueue(parent);
            while (q.Count > 0)
            {
                var currentTransform = q.Dequeue();
                yield return currentTransform;
                foreach (Transform child in currentTransform)
                {
                    q.Enqueue(child);
                }
            }
        }

        public static string GetPath(this Transform current)
        {
            if (current.parent == null)
            {
                return "/" + current.name;
            }

            return current.parent.GetPath() + "/" + current.name;
        }
    }
}