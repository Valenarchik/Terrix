using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using Terrix.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapNetworkManager : NetworkBehaviour
{
    public static BootstrapNetworkManager Instance { get; private set; }

    private void Awake() => Instance = this;

    [ServerRpc(RequireOwnership = false)]
    private void CreateOrJoinDefaultLobby_ToServer(NetworkConnection player)
    {
        if (LobbyManager.Instance.TryGetAvailableLobby(out var availableLobby))
        {
            JoinGame(player, availableLobby.Scene);
        }
        else
        {
            CreateNewGame(player, Terrix.Networking.Scenes.GameScene);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateCustomLobby_ToServer(NetworkConnection player, LobbySettings lobbySettings)
    {
        LobbyManager.Instance.ServerSettingsQueue.Enqueue(lobbySettings);
        CreateNewGame(player, Scenes.GameScene);
    }

    public void CreateOrJoinDefaultLobby_OnClient()
    {
        var player = NetworkManager.ClientManager.Connection; //иногда ошибка
        CloseScenes(new[] { Scenes.MenuScene });
        CreateOrJoinDefaultLobby_ToServer(player);
    }

    public void CreateCustomLobby_OnClient(LobbySettings lobbySettings)
    {
        var player = NetworkManager.ClientManager.Connection;
        CreateCustomLobby_ToServer(player, lobbySettings);
        CloseScenes(new[] { Scenes.MenuScene });
    }

    public void TryJoinCustomLobby(int id)
    {
        var player = NetworkManager.ClientManager.Connection;
        TryJoinGame_ToServer(player, id);
        CloseScenes(new[] { Scenes.MenuScene });
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryJoinGame_ToServer(NetworkConnection player, int id)
    {
        // Scene scene;
        if (LobbyManager.Instance.TryGetCustomLobbyById(id, out var scene))
        {
            JoinGame(player, scene);
        }
        // return true;
    }

    private void CloseScenes(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }


    private void CreateNewGame(NetworkConnection player, string sceneName)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(sceneLookupData);
        sld.Params.ServerParams = new object[] { LobbyManager.Instance.GetLastId() + 1 };
        sld.Options.AllowStacking = true;
        SceneManager.LoadConnectionScenes(player, sld);
    }

    private void JoinGame(NetworkConnection player, Scene scene)
    {
        var sceneLookupData = new SceneLookupData(scene);
        Debug.Log(scene.name);
        SceneLoadData sld = new SceneLoadData(sceneLookupData)
        {
            ReplaceScenes = ReplaceOption.None,
        };
        SceneManager.LoadConnectionScenes(player, sld);
    }
}