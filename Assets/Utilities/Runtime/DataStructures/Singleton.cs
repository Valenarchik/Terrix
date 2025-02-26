using System;

namespace CustomUtilities.DataStructures
{
    public abstract class Singleton<T> where T: Singleton<T>, new()
    {
        public static T Instance { get; private set; }

        protected Singleton()
        {
        }

        public static T CreateInstance()
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException();
            }

            Instance = new T();
            return Instance;
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }
    }
}