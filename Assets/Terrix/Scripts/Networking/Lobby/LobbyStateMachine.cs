using System;

namespace Terrix.Networking

{
    public class LobbyStateMachine
    {
        public LobbySearchingState LobbySearchingState { get; private set; }
        public LobbyStartingState LobbyStartingState { get; private set; }
        public LobbyPlayingState LobbyPlayingState { get; private set; }
        public LobbyEndedState LobbyEndedState { get; private set; }
        public LobbyState CurrentState { get; private set; }
        public event Action<LobbyState> OnStateChanged;

        public LobbyStateMachine(bool isCustom = false)
        {
            LobbySearchingState = isCustom ?new LobbySearchingState(this) : new LobbyTimerSearchingState(this);
            LobbyStartingState = new LobbyStartingState(this);
            LobbyPlayingState = new LobbyPlayingState(this);
            LobbyEndedState = new LobbyEndedState(this);
            CurrentState = LobbySearchingState;
            CurrentState.Enter();
        }

        public void ChangeState(LobbyState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}