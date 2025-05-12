using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Terrix.Networking
{
    public class Lobby : NetworkBehaviour
    {
        public int Id { get; private set; }
        public bool IsCustom { get; private set; }
        public Scene Scene { get; private set; }
        // public List<NetworkObject> Players { get; private set; }
        public List<NetworkConnection> Players { get; private set; }
        public int PlayersMaxCount { get; private set; }
        public int PlayersAndBotsMaxCount { get; private set; }
        public int PlayersCurrentCount => Players.Count;

        public LobbyStateMachine LobbyStateMachine { get; private set; }
        public event Action OnStartInfoSet;
        public event Action OnPlayersChanged;
        public event Action<LobbyStateType> OnStateChanged;
        public event Action<float> OnTimerChanged;
        public event Action<NetworkConnection> OnPlayerExit;

        private void Update()
        {
            if (LobbyStateMachine is null)
            {
                return;
            }

            if (IsServerInitialized)
            {
                LobbyStateMachine.CurrentState.Update();
                float time;
                if (LobbyStateMachine.CurrentState is LobbyTimerState lobbyTimerState)
                {
                    time = lobbyTimerState.TimeToStopTimer;
                }
                else
                {
                    time = 0;
                }

                UpdateTimer_ToObserver(time);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManagerOnRemoteConnectionState_OnServer;
            // Id = GetFreeId();
            Scene = gameObject.scene;
            Players = new List<NetworkConnection>();
            if (LobbyManager.Instance.ServerSettingsQueue.TryDequeue(out var serverSettings))
            {
                Id = LobbyManager.Instance.GetCustomFreeId();
                IsCustom = true;
                PlayersMaxCount = serverSettings.PlayersCount;
                PlayersAndBotsMaxCount = serverSettings.BotsCount + serverSettings.PlayersCount;
                Debug.Log("Custom");
                
                // AddCustomLobbyToLobbyManager();
            }
            else
            {
                Id = LobbyManager.Instance.GetDefaultFreeId();
                PlayersMaxCount = LobbyManager.Instance.PlayersMaxCount;
                PlayersAndBotsMaxCount = LobbyManager.Instance.PlayersAndBotsMaxCount;
                Debug.Log("Default");
                // AddDefaultLobbyToLobbyManager();
            }
            LobbyManager.Instance.AddLobby(Id, this);

            LobbyStateMachine = CreateStateMachine();
            LobbyStateMachine.OnStateChanged += LobbyStateMachineOnStateChanged_OnServer;
            // AddLobbyToLobbyManager();
        }

        protected virtual LobbyStateMachine CreateStateMachine() => new LobbyStateMachine();
        // protected virtual void AddDefaultLobbyToLobbyManager() => LobbyManager.Instance.AddDefaultLobby(Id, this);
        // protected virtual void AddCustomLobbyToLobbyManager() => LobbyManager.Instance.AddCustomLobby(Id, this);

        public override void OnStartClient()
        {
            base.OnStartClient();
            Scene = gameObject.scene;
            var player = NetworkManager.ClientManager.Connection;
            AddPlayer_ToServer(player);
        }

        private void LobbyStateMachineOnStateChanged_OnServer(LobbyState state)
        {
            UpdateStateName_ToObserver(state.LobbyStateType);
        }

        [ObserversRpc]
        private void UpdateStateName_ToObserver(LobbyStateType lobbyStateType)
        {
            OnStateChanged?.Invoke(lobbyStateType);
        }

        private void ServerManagerOnRemoteConnectionState_OnServer(NetworkConnection conn,
            RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Stopped:
                    RemovePlayerFromList_OnServer(conn);
                    break;
                case RemoteConnectionState.Started:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        private void AddPlayer_ToServer(NetworkConnection newPlayer)
        {
            Players.Add(newPlayer);
            SetInfo_ToTarget(newPlayer, Id, PlayersMaxCount, IsCustom);
            UpdatePlayers_ToObserver(Players);
            UpdateStateName_ToObserver(LobbyStateMachine.CurrentState.LobbyStateType);
            if (Players.Count == PlayersMaxCount)
            {
                LobbyStateMachine.ChangeState(LobbyStateMachine.LobbyBeforeStartingState);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemovePlayer_ToServer(NetworkConnection player)
        {
            var sud = new SceneUnloadData(new[] { Scenes.GameScene });
            SceneManager.UnloadConnectionScenes(player, sud);
            var sld = new SceneLoadData(new[] { Scenes.MenuScene });
            SceneManager.LoadConnectionScenes(player, sld);
            RemovePlayerFromList_OnServer(player);
        }

        [TargetRpc]
        private void SetInfo_ToTarget(NetworkConnection connection, int id, int playersMaxCount, bool isCustom)
        {
            Id = id;
            IsCustom = isCustom;
            PlayersMaxCount = playersMaxCount;
            OnStartInfoSet?.Invoke();
        }

        [ObserversRpc]
        private void UpdatePlayers_ToObserver(List<NetworkConnection> players)
        {
            Players = players;
            OnPlayersChanged?.Invoke();
        }

        [ObserversRpc]
        private void UpdateTimer_ToObserver(float timerTime)
        {
            OnTimerChanged?.Invoke(timerTime);
        }

        public bool IsAvailableForJoin() => LobbyStateMachine.CurrentState == LobbyStateMachine.LobbySearchingState;

        public void RemoveThisPlayer_OnClient()
        {
            RemovePlayer_ToServer(NetworkManager.ClientManager.Connection);
        }

        private void RemovePlayerFromList_OnServer(NetworkConnection player)
        {
            Players.Remove(player);
            if (Players.Count == 0)
            {
                LobbyManager.Instance.RemoveLobby(Id);
            }

            UpdatePlayers_ToObserver(Players);
            OnPlayerExit?.Invoke(player);
        }
    }
}