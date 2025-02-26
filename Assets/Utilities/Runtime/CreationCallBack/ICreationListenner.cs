namespace CustomUtilities.CreationCallBack
{
    public interface ICreationCallback
    {
        /// <summary>
        /// Вызывается после Awake и OnEnable, если объект был активен.
        /// Вызывается до Awake и OnEnable, если объект был неактивен.
        /// </summary>
        public void OnCreation();
    }

    public interface ICreationCallback<in T1>
    {
        /// <summary>
        /// Вызывается после Awake и OnEnable, если объект был активен.
        /// Вызывается до Awake и OnEnable, если объект был неактивен.
        /// </summary>
        public void OnCreation(T1 param1);
    }

    public interface ICreationCallback<in T1, in T2>
    {
        /// <summary>
        /// Вызывается после Awake и OnEnable, если объект был активен.
        /// Вызывается до Awake и OnEnable, если объект был неактивен.
        /// </summary>
        public void OnCreation(T1 param1, T2 param2);
    }

    public interface ICreationCallback<in T1, in T2, in T3>
    {
        /// <summary>
        /// Вызывается после Awake и OnEnable, если объект был активен.
        /// Вызывается до Awake и OnEnable, если объект был неактивен.
        /// </summary>
        public void OnCreation(T1 param1, T2 param2, T3 param3);
    }

    public interface ICreationCallback<in T1, in T2, in T3, in T4>
    {
        /// <summary>
        /// Вызывается после Awake и OnEnable, если объект был активен.
        /// Вызывается до Awake и OnEnable, если объект был неактивен.
        /// </summary>
        public void OnCreation(T1 param1, T2 param2, T3 param3, T4 param4);
    }
}