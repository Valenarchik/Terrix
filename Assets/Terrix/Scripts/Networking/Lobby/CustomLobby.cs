using FishNet.Connection;
using FishNet.Object;

namespace Terrix.Networking
{
    public class CustomLobby : Lobby
    {
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
                if (LobbyStateMachine.CurrentState == LobbyStateMachine.LobbyStartingState)
                {
                    time = LobbyStateMachine.LobbyStartingState.TimeToStopTimer;
                }
                else
                {
                    time = 0;
                }

                UpdateTimer_ToObserver(time);
            }
        }

        protected override int GetDefaultFreeId() => LobbyManager.Instance.GetCustomFreeId();
        protected override LobbyStateMachine CreateStateMachine() => new LobbyStateMachine(true);
        protected override void AddDefaultLobbyToLobbyManager() => LobbyManager.Instance.AddCustomLobby(Id, this);


        [ServerRpc(RequireOwnership = false)]
        protected override void AddPlayer_ToServer(NetworkConnection newPlayer)
        {
            Players.Add(newPlayer);
            SetInfo_ToTarget(newPlayer, Id, PlayersMaxCount);
            UpdatePlayers_ToObserver(Players);
            UpdateStateName_ToObserver(LobbyStateMachine.CurrentState.LobbyStateType);
        }
    }
}