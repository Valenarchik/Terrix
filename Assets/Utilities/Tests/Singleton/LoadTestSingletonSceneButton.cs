using UnityEngine.SceneManagement;

namespace CustomUtilities.Tests
{
    public class LoadTestSingletonSceneButton: ButtonClickHandler
    {
        protected override void OnClick()
        {
            SceneManager.LoadScene("TestSingleton");
        }
    }
}