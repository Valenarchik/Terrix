using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtilities.CreationCallBack
{
    public static class InstantiateExtensionsZero
    {
        public static T Instantiate<T>([NotNull] this T obj)
            where T : Object, ICreationCallback
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return InstantiateInternal(() => Object.Instantiate(obj));
        }

        public static T Instantiate<T>([NotNull] this T obj, [NotNull] Transform parent)
            where T : Object, ICreationCallback
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, parent));
        }

        public static T Instantiate<T>([NotNull] this T obj, [NotNull] Transform parent, bool instantiateInWorldSpace)
            where T : Object, ICreationCallback
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, parent, instantiateInWorldSpace));
        }

        public static T Instantiate<T>([NotNull] this T obj, Vector3 position, Quaternion quaternion)
            where T : Object, ICreationCallback
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, position, quaternion));
        }

        public static T Instantiate<T>([NotNull] this T obj,
            Vector3 position,
            Quaternion quaternion,
            [NotNull] Transform parent)
            where T : Object, ICreationCallback
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, position, quaternion, parent));
        }


        private static T InstantiateInternal<T>(Func<T> fabricFunc) where T : Object, ICreationCallback
        {
            var instance = fabricFunc.Invoke();
            instance.OnCreation();
            return instance;
        }
    }
}