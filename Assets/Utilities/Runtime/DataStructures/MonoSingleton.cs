using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUtilities.DataStructures
{
    public class MonoSingleton<T> : MonoBehaviour
        where T : MonoSingleton<T>
    {
        private static T instance;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        /// true когда объект инициализируется или при первом запросе к нему
        /// false когда сцена выгружается
        /// </summary>
        private static bool initialized;

        public static T Instance
        {
            get
            {
                if (initialized)
                {
                    return instance;
                }

                var objs = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
                switch (objs.Length)
                {
                    case 0:
                    {
                        return null;
                    }
                    case > 1:
                    {
                        throw new InvalidMonoSingletonException($"Duplicated {typeof(T)} singleton object in this scene!");
                    }
                }

                objs[0].Initialize();

                return instance;
            }
        }
        
        public static bool Exist => instance != null;
        public static bool NotExist => !Exist;

        static MonoSingleton()
        {
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
        }
        
        private static void SceneManagerOnSceneUnloaded(Scene scene)
        {
            initialized = false;
            instance = null;
        }
        
        private void Awake()
        {
            if (GetType() != typeof(T))
            {
                throw new InvalidMonoSingletonException($"The singleton object {typeof(T)} does not refer to himself");
            }

            if (instance == null)
            {
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(this);
                throw new InvalidMonoSingletonException($"Duplicated {typeof(T)} singleton object in this scene!");
            }
            
            AwakeInternal();
        }

        private void Initialize()
        {
            instance = (T) this;
            initialized = true;
            InitializeInternal();
        }

        /// <summary>
        /// Тот же Awake
        /// </summary>
        protected virtual void AwakeInternal()
        {
        }
        
        /// <summary>
        /// Используй это когда надо 100% вызов инициализации, т.к. AwakeInternal не вызывается на выключенном объекте. 
        /// </summary>
        protected virtual void InitializeInternal()
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