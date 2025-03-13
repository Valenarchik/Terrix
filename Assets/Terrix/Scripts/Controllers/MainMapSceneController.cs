using CustomUtilities.DataStructures;

namespace Terrix.Controllers
{
    public class MainMapSceneController: MonoSingleton<MainMapSceneController>
    {
        private void Start()
        {
            MainMapEvents.CreateInstance();
            MainMapEvents.Instance.SetSceneReady();
        }

        protected override void OnDestroyInternal()
        {
            MainMapEvents.ReleaseInstance();
        }
    }
}