using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine.SceneManagement;

namespace Terrix.Networking
{
    public class Lobby : NetworkBehaviour
    {
        public int Id { get; private set; }
        public Scene Scene { get; private set; }
        // public List<NetworkObject> Players { get; private set; }
        public List<NetworkConnection> Players { get; private set; }
        public int PlayersMaxCount { get; private set; }
        public int PlayersAndBotsMaxCount { get; private set; }
        public int PlayersCurrentCount => Players.Count;

        public LobbyStateMachine LobbyStateMachine { get; private set; }
        public event Action OnStartInfoSet;
        public event Action OnPlayersChanged;
        public event Action<string> OnStateChanged;
        public event Action<float> OnTimerChanged;

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
            Id = GetFreeId();
            Scene = gameObject.scene;
            Players = new List<NetworkConnection>();
            PlayersMaxCount = LobbyManager.Instance.PlayersMaxCount;
            PlayersAndBotsMaxCount = LobbyManager.Instance.PlayersAndBotsMaxCount;
            LobbyStateMachine = CreateStateMachine();
            LobbyStateMachine.OnStateChanged += LobbyStateMachineOnStateChanged_OnServer;
            AddLobbyToLobbyManager();
        }

        protected virtual int GetFreeId() => LobbyManager.Instance.GetFreeId();

        protected virtual LobbyStateMachine CreateStateMachine() => new LobbyStateMachine();
        protected virtual void AddLobbyToLobbyManager() => LobbyManager.Instance.AddDefaultLobby(Id, this);

        public override void OnStartClient()
        {
            base.OnStartClient();
            Scene = gameObject.scene;
            var player = NetworkManager.ClientManager.Connection;
            AddPlayer_ToServer(player);
        }

        protected void LobbyStateMachineOnStateChanged_OnServer(LobbyState state)
        {
            UpdateStateName_ToObserver(state.Name);
        }

        [ObserversRpc]
        protected void UpdateStateName_ToObserver(string lobbyStateName)
        {
            OnStateChanged?.Invoke(lobbyStateName);
        }

        protected void ServerManagerOnRemoteConnectionState_OnServer(NetworkConnection conn,
            RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Stopped:
                    Players.Remove(conn);
                    if (Players.Count == 0)
                    {
                        LobbyManager.Instance.RemoveDefaultLobby(Id);
                    }

                    UpdatePlayers_ToObserver(Players);
                    break;
                case RemoteConnectionState.Started:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        protected virtual void AddPlayer_ToServer(NetworkConnection newPlayer)
        {
            Players.Add(newPlayer);
            SetInfo_ToTarget(newPlayer, Id, PlayersMaxCount);
            UpdatePlayers_ToObserver(Players);
            UpdateStateName_ToObserver(LobbyStateMachine.CurrentState.ToString());
            if (Players.Count == PlayersMaxCount)
            {
                LobbyStateMachine.ChangeState(LobbyStateMachine.LobbyBeforeStartingState);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void RemovePlayer_ToServer(NetworkConnection player)
        {
            Players.Remove(player);
            var sld = new SceneLoadData(new[] { Scenes.MenuScene });
            SceneManager.LoadConnectionScenes(player, sld);
            UpdatePlayers_ToObserver(Players);
        }

        [TargetRpc]
        protected void SetInfo_ToTarget(NetworkConnection connection, int id, int playersMaxCount)
        {
            Id = id;
            PlayersMaxCount = playersMaxCount;
            OnStartInfoSet?.Invoke();
        }

        [ObserversRpc]
        protected void UpdatePlayers_ToObserver(List<NetworkConnection> players)
        {
            Players = players;
            OnPlayersChanged?.Invoke();
        }

        [ObserversRpc]
        protected void UpdateTimer_ToObserver(float timerTime)
        {
            OnTimerChanged?.Invoke(timerTime);
        }

        public bool IsAvailableForJoin() => LobbyStateMachine.CurrentState == LobbyStateMachine.LobbySearchingState;

        public void EndGame()
        {
            EndGame_ToServer();
        }

        [ServerRpc(RequireOwnership = false)]
        void EndGame_ToServer()
        {
            LobbyStateMachine.ChangeState(LobbyStateMachine.LobbyEndedState);
            EndGame_ToObserver();
        }

        [ObserversRpc]
        void EndGame_ToObserver()
        {
            // endGameButton.gameObject.SetActive(false);
            // leaveLobbyButton.gameObject.SetActive(true);
        }


        public void LeaveLobby()
        {
            Unsubscribe_ToServer();
            RemovePlayer_ToServer(NetworkManager.ClientManager.Connection);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(Scenes.GameScene);
        }

        [ServerRpc(RequireOwnership = false)]
        void Unsubscribe_ToServer()
        {
            NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManagerOnRemoteConnectionState_OnServer;
        }
    }
}