using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private void Start()
    {
        if (networkManager.ClientManager.StartConnection())
        {
            GoToMenu();
        }
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene(Terrix.Networking.Scenes.MenuScene, LoadSceneMode.Additive);
        // SceneManager.LoadScene(Terrix.Networking.Scenes.GameScene, LoadSceneMode.Additive);
    }
}