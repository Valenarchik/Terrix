using System;
using CustomUtilities.DataStructures;

namespace Terrix.Controllers
{
    public class MainMapEvents : Singleton<MainMapEvents>
    {
        private bool sceneIsReady;
        private Action onSceneReady;

        public void CheckSceneReady(Action onSceneReady)
        {
            if (sceneIsReady)
            {
                onSceneReady?.Invoke();
                return;
            }

            this.onSceneReady += onSceneReady;
        }

        public void SetSceneReady()
        {
            if (sceneIsReady)
            {
                return;
            }
            sceneIsReady = true;
            onSceneReady?.Invoke();
        }
    }
}