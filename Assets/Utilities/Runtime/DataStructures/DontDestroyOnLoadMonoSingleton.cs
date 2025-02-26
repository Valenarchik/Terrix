using UnityEngine;

namespace CustomUtilities.DataStructures
{
    public abstract class DontDestroyOnLoadMonoSingleton<T> : MonoBehaviour
        where T : DontDestroyOnLoadMonoSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (NotExist)
                {
                    throw new InvalidMonoSingletonException(
                        $"The dont destroy on load singleton object {typeof(T)} does not exist." +
                        "Please move this object to top hierarchy and activate yourself.");
                }

                return instance;
            }
        }

        public static bool Exist => instance != null;
        public static bool NotExist => !Exist;

        private void Awake()
        {
            if (GetType() != typeof(T))
            {
                throw new InvalidMonoSingletonException(
                    $"The dont destroy on load singleton object {typeof(T)} does not refer to himself");
            }

            if (transform.parent != null)
            {
                throw new InvalidMonoSingletonException(
                    $"The dont destroy on load singleton object {typeof(T)} must not have a parent");
            }

            if (instance == null)
            {
                instance = (T) this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            OnAwakeInternal();
        }

        /// <summary>
        /// Тот же Awake
        /// </summary>
        protected virtual void OnAwakeInternal()
        {
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            OnDestroyInternal();
        }

        /// <summary>
        /// Тот жe OnDestroy
        /// </summary>
        protected virtual void OnDestroyInternal()
        {
        }
    }
}