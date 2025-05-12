using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using Terrix.Game.GameRules;
using Terrix.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private int playersMaxCount;
    [SerializeField] private int playersAndBotsMaxCount;
    public int PlayersMaxCount => playersMaxCount;
    public int PlayersAndBotsMaxCount => playersAndBotsMaxCount;
    private Dictionary<int, Lobby> defaultLobbies = new();
    private Dictionary<int, Lobby> customLobbies = new();
    // private Dictionary<int, CustomLobby> customLobbies = new();
    public Queue<LobbySettings> ServerSettingsQueue { get; private set; } = new();
    public static LobbyManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI lobbiesCountText;

    private void Awake() => Instance = this;
    private TaskCompletionSource<bool> canJoinCustomLobbyTcs;


    public bool TryGetAvailableLobby(out Lobby availableLobby)
    {
        foreach (var lobby in defaultLobbies.Values)
        {
            if (lobby.IsAvailableForJoin())
            {
                availableLobby = lobby;
                return true;
            }
        }

        availableLobby = null;
        return false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateDefaultLobbies_ToServer();
    }

    [ObserversRpc]
    private void UpdateDefaultLobbies_ToObserver(Dictionary<int, Lobby> newLobbies)
    {
        defaultLobbies = newLobbies;
    }

    // private void UpdateCustomLobbies_ToObserver(Dictionary<int, CustomLobby> newLobbies)
    [ObserversRpc]
    private void UpdateCustomLobbiesCount_ToObserver(int count)
    {
        lobbiesCountText.text = $"Количество лобби на сервере: {count.ToString()}";
    }

    // public void AddDefaultLobby(int id, Lobby lobby)
    // {
    //     defaultLobbies.Add(id, lobby);
    //     UpdateDefaultLobbies_ToObserver(defaultLobbies);
    // }
    //
    // // public void AddCustomLobby(int id, CustomLobby lobby)
    // public void AddCustomLobby(int id, Lobby lobby)
    // {
    //     customLobbies.Add(id, lobby);
    //     UpdateCustomLobbiesCount_ToObserver(customLobbies.Count);
    // }

    public void AddLobby(int id, Lobby lobby)
    {
        if (lobby.IsCustom)
        {
            customLobbies.Add(id, lobby);
        }
        else
        {
            defaultLobbies.Add(id, lobby);
        }

        UpdateCustomLobbiesCount_ToObserver(customLobbies.Count);
    }


    public void RemoveLobby(int id)
    {
        if (TryGetCustomLobbyById(id, out var _))
        {
            customLobbies.Remove(id);
            UpdateCustomLobbiesCount_ToObserver(customLobbies.Count);
        }
        else
        {
            defaultLobbies.Remove(id);
            UpdateDefaultLobbies_ToObserver(defaultLobbies);
        }
        // UpdateDefaultLobbies_ToObserver(defaultLobbies);
    }

    public int GetDefaultFreeId()
    {
        var id = GetLastId() + 1;
        while (defaultLobbies.ContainsKey(id))
        {
            id++;
        }

        return id;
    }

    public int GetCustomFreeId()
    {
        var id = Random.Range(100000, 1000000);
        while (customLobbies.ContainsKey(id))
        {
            id++;
        }

        return id;
    }

    public int GetLastId()
    {
        UpdateDefaultLobbies_ToServer();
        if (!defaultLobbies.Any())
        {
            return 0;
        }

        return defaultLobbies.Keys.Last();
    }

    public bool TryGetCustomLobbyById(int id, out Scene scene)
    {
        UpdateCustomLobbies_ToServer();
        if (customLobbies.TryGetValue(id, out var lobby))
        {
            scene = lobby.gameObject.scene;
            return true;
        }

        scene = new Scene();
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateDefaultLobbies_ToServer()
    {
        UpdateDefaultLobbies_ToObserver(defaultLobbies);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateCustomLobbies_ToServer()
    {
        UpdateCustomLobbiesCount_ToObserver(customLobbies.Count);
    }
    public void CreateOrJoinDefaultLobby_OnClient()
    {
        var player = NetworkManager.ClientManager.Connection; //иногда ошибка
        CloseScenes(new[] { Scenes.MenuScene });
        CreateOrJoinDefaultLobby_ToServer(player);
    }
    [ServerRpc(RequireOwnership = false)]

    private void CreateOrJoinDefaultLobby_ToServer(NetworkConnection player)
    {
        if (TryGetAvailableLobby(out var availableLobby))
        {
            JoinLobby(player, availableLobby.Scene);
        }
        else
        {
            CreateNewLobby(player, Scenes.GameScene);
        }
    }
    public void CreateCustomLobby_OnClient(LobbySettings lobbySettings)
    {
        var player = NetworkManager.ClientManager.Connection;
        CreateCustomLobby_ToServer(player, lobbySettings);
        CloseScenes(new[] { Scenes.MenuScene });
    }
    [ServerRpc(RequireOwnership = false)]
    private void CreateCustomLobby_ToServer(NetworkConnection player, LobbySettings lobbySettings)
    {
        ServerSettingsQueue.Enqueue(lobbySettings);
        CreateNewLobby(player, Scenes.GameScene);
    }
    private void JoinLobby(NetworkConnection player, Scene scene)
    {
        var sceneLookupData = new SceneLookupData(scene);
        var sld = new SceneLoadData(sceneLookupData)
        {
            ReplaceScenes = ReplaceOption.None,
        };
        SceneManager.LoadConnectionScenes(player, sld);
    }
    private void CreateNewLobby(NetworkConnection player, string sceneName)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(sceneLookupData);
        // sld.Params.ServerParams = new object[] {GetLastId() + 1 };
        sld.Options.AllowStacking = true;
        SceneManager.LoadConnectionScenes(player, sld);
    }
    public void CloseScenes(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }
    public async void TryJoinCustomLobby_OnClient(int id)
    {
        var player = NetworkManager.ClientManager.Connection;
        canJoinCustomLobbyTcs = new TaskCompletionSource<bool>();

        TryJoinCustomLobby_ToServer(player, id);
        var result = await canJoinCustomLobbyTcs.Task;

        if (result)
        {
            CloseScenes(new[] { Scenes.MenuScene });
        }
        else
        {
            MainMenuManager.Instance.ShowCustomLobbyErrorText("Лобби с таким ID отсутствует");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void TryJoinCustomLobby_ToServer(NetworkConnection connection, int id)
    {
        if (TryGetCustomLobbyById(id, out var scene))
        {
            TryJoinLobby_ToTarget(connection, true);
            JoinLobby(connection, scene);
        }
        else
        {
            TryJoinLobby_ToTarget(connection, false);
        }
    }
    [TargetRpc]
    private void TryJoinLobby_ToTarget(NetworkConnection connection, bool result)
    {
        canJoinCustomLobbyTcs.SetResult(result);
    }
    public void CreateFakeLobby_OnClient(int botsCount)
    {
        CreateFakeLobby_ToServer(botsCount);
    }
    [ServerRpc(RequireOwnership = false)]
    private void CreateFakeLobby_ToServer(int botsCount)
    {
        ServerSettingsQueue.Enqueue(new LobbySettings(0, botsCount));
        CreateFakeGame_OnServer(Scenes.GameScene);
    }
    private void CreateFakeGame_OnServer(string sceneName)
    {
        var sceneLookupData = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(sceneLookupData);
        sld.Params.ServerParams = new object[] {GetLastId() + 1 };
        sld.Options.AllowStacking = true;
        SceneManager.LoadConnectionScenes(sld);
    }
}