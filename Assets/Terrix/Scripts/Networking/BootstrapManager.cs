using System;
using System.Threading.Tasks;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
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
    }

}