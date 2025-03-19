using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using Terrix.Game.GameRules;
using Terrix.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int playersMaxCount;
    public int PlayersMaxCount => playersMaxCount;
    private Dictionary<int, Lobby> _lobbies = new();
    public static LobbyManager Instance { get; private set; }

    private void Awake() => Instance = this;
    // {
    //     if (!IsServerInitialized)
    //     {
    //         return;
    //     }
    //
    //     Instance = this;
    // }


    // Update is called once per frame
    void Update()
    {
    }

    // public void AddScene(int id, Scene scene)
    // {
    //     _scenes.Add(id, scene);
    // }
    public bool TryGetAvailableLobby(out Lobby availableLobby)
    {
        foreach (var lobby in _lobbies.Values)
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
        UpdateLobbies_ToServer();
    }

    [ObserversRpc]
    void UpdateLobbies_ToObserver(Dictionary<int, Lobby> newLobbies)
    {
        _lobbies = newLobbies;
    }

    public void AddLobby(int id, Lobby lobby)
    {
        _lobbies.Add(id, lobby);
        UpdateLobbies_ToObserver(_lobbies);
    }
    public void RemoveLobby(int id)
    {
        _lobbies.Remove(id);
        UpdateLobbies_ToObserver(_lobbies);
    }

    public int GetFreeId()
    {
        var id = GetLastId() + 1;
        while (_lobbies.ContainsKey(id))
        {
            id++;
        }

        return id;
    }

    public int GetLastId()
    {
        UpdateLobbies_ToServer();
        if (!_lobbies.Any())
        {
            return 0;
        }
        return _lobbies.Keys.Last();
    }

    public Scene GetSceneById(int id)
    {
        UpdateLobbies_ToServer();
        if (_lobbies.ContainsKey(id))
        {
            return _lobbies[id].gameObject.scene;
        }

        throw new Exception("there is no such id");
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateLobbies_ToServer()
    {
        UpdateLobbies_ToObserver(_lobbies);
    }
}