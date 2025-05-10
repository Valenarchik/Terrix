using System;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Managing.Timing;
using FishNet.Object;
using Terrix.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapNetworkManager : NetworkBehaviour
{
    public static BootstrapNetworkManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private TextMeshProUGUI ramText;
    [SerializeField] private TextMeshProUGUI connectionsCountText;
    private TimeManager timeManager;

    private void Awake() => Instance = this;

    private void Update()
    {
        var ping = NetworkManager.TimeManager.RoundTripTime;
        var text = $"Ping: {ping}";
        pingText.text = text;
        var usedRam = System.GC.GetTotalMemory(false) / 1024 / 1024;
        ramText.text = $"Used RAM: {usedRam}";
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
            CreateNewGame_OnServer(player, Terrix.Networking.Scenes.GameScene);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateFakeLobby_ToServer(int botsCount)
    {
        LobbyManager.Instance.ServerSettingsQueue.Enqueue(new LobbySettings(0, botsCount));
        CreateFakeGame_OnServer(Scenes.GameScene);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateCustomLobby_ToServer(NetworkConnection player, LobbySettings lobbySettings)
    {
        LobbyManager.Instance.ServerSettingsQueue.Enqueue(lobbySettings);
        CreateNewGame_OnServer(player, Scenes.GameScene);
    }

    public void CreateOrJoinDefaultLobby_OnClient()
    {
        var player = NetworkManager.ClientManager.Connection; //иногда ошибка
        CloseScenes(new[] { Scenes.MenuScene });
        CreateOrJoinDefaultLobby_ToServer(player);
    }

    public void CreateFakeLobby_OnClient(int botsCount)
    {
        CreateFakeLobby_ToServer(botsCount);
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


    private void CreateNewGame_OnServer(NetworkConnection player, string sceneName)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(sceneLookupData);
        sld.Params.ServerParams = new object[] { LobbyManager.Instance.GetLastId() + 1 };
        sld.Options.AllowStacking = true;
        SceneManager.LoadConnectionScenes(player, sld);
    }

    private void CreateFakeGame_OnServer(string sceneName)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(sceneLookupData);
        sld.Params.ServerParams = new object[] { LobbyManager.Instance.GetLastId() + 1 };
        sld.Options.AllowStacking = true;
        SceneManager.LoadConnectionScenes(sld);
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

    public override void OnStartClient()
    {
        NetworkManager.
        StartCoroutine(MeasurePing());
    }

    public override void OnStartServer()
    {
        StartCoroutine(MeasurePing_OnServer());
    }

    private System.Collections.IEnumerator MeasurePing()
    {
        while (true)
        {
            var time = Time.time;

            Ping_ToServer(NetworkManager.ClientManager.Connection, time);

            yield return new WaitForSeconds(1f);
        }
    }

    private System.Collections.IEnumerator MeasurePing_OnServer()
    {
        while (true)
        {
            UpdateConnectionCount_ToObserver(NetworkManager.ServerManager.Clients.Count);
            yield return new WaitForSeconds(1f);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void Ping_ToServer(NetworkConnection conn, float time)
    {
        Ping_ToClient(conn, time);
    }

    [TargetRpc]
    private void Ping_ToClient(NetworkConnection conn, float time)
    {
        // Debug.Log(NetworkManager.TimeManager.PingInterval);
        // var ping = (Time.time - time) * 1000;
        // var text = $"Ping: {ping}";
        // pingText.text = text;
        // Debug.Log(text);
    }

    [ObserversRpc]
    private void UpdateConnectionCount_ToObserver(int count)
    {
        connectionsCountText.text = $"Connections count: {count}";
    }
}