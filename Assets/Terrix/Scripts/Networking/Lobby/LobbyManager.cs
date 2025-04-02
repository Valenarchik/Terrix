using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using Terrix.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LobbyManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int playersMaxCount;
    [SerializeField] private int playersAndBotsMaxCount;
    public int PlayersMaxCount => playersMaxCount;
    public int PlayersAndBotsMaxCount => playersAndBotsMaxCount;
    private Dictionary<int, Lobby> defaultLobbies = new();
    private Dictionary<int, CustomLobby> customLobbies = new();
    public static LobbyManager Instance { get; private set; }

    private void Awake() => Instance = this;
    
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

    private void UpdateCustomLobbies_ToObserver(Dictionary<int, CustomLobby> newLobbies)
    {
        customLobbies = newLobbies;
    }

    public void AddDefaultLobby(int id, Lobby lobby)
    {
        defaultLobbies.Add(id, lobby);
        UpdateDefaultLobbies_ToObserver(defaultLobbies);
    }

    public void AddCustomLobby(int id, CustomLobby lobby)
    {
        customLobbies.Add(id, lobby);
        UpdateCustomLobbies_ToObserver(customLobbies);
    }

    public void RemoveDefaultLobby(int id)
    {
        defaultLobbies.Remove(id);
        UpdateDefaultLobbies_ToObserver(defaultLobbies);
    }

    public int GetFreeId()
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
        UpdateCustomLobbies_ToObserver(customLobbies);
    }
}