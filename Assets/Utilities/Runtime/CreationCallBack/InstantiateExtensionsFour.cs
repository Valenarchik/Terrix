using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtilities.CreationCallBack
{
    public static class InstantiateExtensionsFour
    {
        public static T Instantiate<T, T1, T2, T3, T4>([NotNull] this T obj, T1 param1, T2 param2, T3 param3, T4 param4)
            where T : Object, ICreationCallback<T1, T2, T3, T4>
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return InstantiateInternal(() => Object.Instantiate(obj), param1, param2, param3, param4);
        }

        public static T Instantiate<T, T1, T2, T3, T4>([NotNull] this T obj,
            [NotNull] Transform parent,
            T1 param1,
            T2 param2,
            T3 param3,
            T4 param4)
            where T : Object, ICreationCallback<T1, T2, T3, T4>
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, parent), param1, param2, param3, param4);
        }

        public static T Instantiate<T, T1, T2, T3, T4>([NotNull] this T obj,
            [NotNull] Transform parent,
            bool instantiateInWorldSpace,
            T1 param1,
            T2 param2,
            T3 param3,
            T4 param4)
            where T : Object, ICreationCallback<T1, T2, T3, T4>
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, parent, instantiateInWorldSpace),
                param1,
                param2,
                param3,
                param4);
        }

        public static T Instantiate<T, T1, T2, T3, T4>([NotNull] this T obj,
            Vector3 position,
            Quaternion quaternion,
            T1 param1,
            T2 param2,
            T3 param3,
            T4 param4)
            where T : Object, ICreationCallback<T1, T2, T3, T4>
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, position, quaternion),
                param1,
                param2,
                param3,
                param4);
        }

        public static T Instantiate<T, T1, T2, T3, T4>([NotNull] this T obj,
            Vector3 position,
            Quaternion quaternion,
            [NotNull] Transform parent,
            T1 param1,
            T2 param2,
            T3 param3,
            T4 param4)
            where T : Object, ICreationCallback<T1, T2, T3, T4>
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return InstantiateInternal(() => Object.Instantiate(obj, position, quaternion, parent),
                param1,
                param2,
                param3,
                param4);
        }


        private static T InstantiateInternal<T, T1, T2, T3, T4>(Func<T> fabricFunc,
            T1 param1,
            T2 param2,
            T3 param3,
            T4 param4)
            where T : Object, ICreationCallback<T1, T2, T3, T4>
        {
            var instance = fabricFunc.Invoke();
            instance.OnCreation(param1, param2, param3, param4);
            return instance;
        }
    }
}