using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapNetworkManager : NetworkBehaviour
{
    public static BootstrapNetworkManager Instance { get; private set; }

    private void Awake() => Instance = this;


    public void CreateNewLobby(string sceneName, string[] scenesToClose)
    {
        var player = NetworkManager.ClientManager.Connection;
        CreateNewGame_ToServer(player, sceneName);
        CloseScenesOld(scenesToClose);
    }

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
    private void CreateCustomLobby_ToServer(NetworkConnection player)
    {
        CreateNewGame(player, Terrix.Networking.Scenes.CustomGameScene);
    }

    public void CreateOrJoinDefaultLobby_OnClient()
    {
        var player = NetworkManager.ClientManager.Connection;
        CreateOrJoinDefaultLobby_ToServer(player);
        CloseScenesOld(new[] { Terrix.Networking.Scenes.MenuScene });
    }

    public void CreateCustomLobby_OnClient()
    {
        var player = NetworkManager.ClientManager.Connection;
        CreateCustomLobby_ToServer(player);
        CloseScenesOld(new[] { Terrix.Networking.Scenes.MenuScene });
    }

    public void TryJoinCustomLobby(int id)
    {
        var player = NetworkManager.ClientManager.Connection;
        // var isSucceed = TryJoinGame_ToServer(player, id);
        // if (TryJoinGame_ToServer(player, id, out var isRob))
        // {
        //     CloseScenesOld(new[] { Terrix.Networking.Scenes.MenuScene });
        //     return true;
        // }
        //
        // return false;
        TryJoinGame_ToServer(player, id);
        CloseScenesOld(new[] { Terrix.Networking.Scenes.MenuScene });
    }


    [ServerRpc(RequireOwnership = false)]
    void CreateNewGame_ToServer(NetworkConnection player, string sceneName)
    {
        CreateNewGame(player, sceneName);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryJoinGame_ToServer(NetworkConnection player, int id)
    {
        // Scene scene;
        if (!LobbyManager.Instance.TryGetCustomLobbyById(id, out var scene))
        {
        }

        JoinGame(player, scene);
        // return true;
    }

    private void CloseScenesOld(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }

    // private IEnumerator CloseScenesDelayed(string[] scenesToClose, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     CloseScenesOld(scenesToClose);
    // }

    // [TargetRpc]
    // private void CloseScenes(NetworkConnection connection, string[] scenesToClose)
    // {
    //     var sceneUnloadData = new SceneUnloadData(scenesToClose);
    //     InstanceFinder.SceneManager.UnloadConnectionScenes(connection, sceneUnloadData);
    // }


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