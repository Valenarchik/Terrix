using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private NetworkManager networkManagerPrefab;

    private void Start()
    {
        if (networkManager.ClientManager.StartConnection())
        {
            GoToMenu();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            var newPlayer = Instantiate(networkManagerPrefab);
            newPlayer.ClientManager.StartConnection();
        }
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene(Terrix.Networking.Scenes.MenuScene, LoadSceneMode.Additive);
        // SceneManager.LoadScene(Terrix.Networking.Scenes.GameScene, LoadSceneMode.Additive);
    }
}