using System;

namespace Terrix.Networking

{
    public class LobbyStateMachine
    {
        // public LobbySearchingState LobbySearchingState { get; private set; }
        // public LobbyBeforeStartingState LobbyBeforeStartingState { get; private set; }
        // public LobbyStartingState LobbyStartingState { get; private set; }
        // public LobbyPlayingState LobbyPlayingState { get; private set; }
        // public LobbyEndedState LobbyEndedState { get; private set; }
        // public LobbyState CurrentState { get; private set; }
        // public event Action<LobbyState> OnStateChanged;
        //
        // public LobbyStateMachine(bool isCustom = false)
        // {
        //     LobbySearchingState = isCustom ? new LobbySearchingState(this) : new LobbyTimerSearchingState(this);
        //     LobbyBeforeStartingState = new LobbyBeforeStartingState(this);
        //     LobbyStartingState = new LobbyStartingState(this);
        //     LobbyPlayingState = new LobbyPlayingState(this);
        //     LobbyEndedState = new LobbyEndedState(this);
        //     CurrentState = LobbySearchingState;
        //     CurrentState.Enter();
        // }
        //
        // public void ChangeState(LobbyState newState)
        // {
        //     CurrentState.Exit();
        //     CurrentState = newState;
        //     CurrentState.Enter();
        //     OnStateChanged?.Invoke(CurrentState);
        // }
        public LobbyState LobbySearchingState { get; private set; }
        public LobbyTimerState LobbyBeforeStartingState { get; private set; }
        public LobbyTimerState LobbyStartingState { get; private set; }
        public LobbyState LobbyPlayingState { get; private set; }
        public LobbyState LobbyEndedState { get; private set; }
        public LobbyState CurrentState { get; private set; }
        public event Action<LobbyState> OnStateChanged;

        public LobbyStateMachine(bool isCustom = false)
        {
            LobbySearchingState = isCustom
                ? new LobbyState(this, "Поиск игроков")
                : new LobbyTimerState(this, "Поиск игроков", 90);
            LobbyBeforeStartingState = new LobbyTimerState(this, "Игра начинается", 5);
            LobbyStartingState = new LobbyTimerState(this, "Выбор территории", 30);
            LobbyPlayingState = new LobbyState(this, "Игра идёт");
            LobbyEndedState = new LobbyState(this, "Игра окончена");
            CurrentState = LobbySearchingState;
            if (LobbySearchingState is LobbyTimerState lobbyTimerState)
            {
                lobbyTimerState.SetNextState(LobbyBeforeStartingState);
            }

            LobbyBeforeStartingState.SetNextState(LobbyStartingState);
            LobbyStartingState.SetNextState(LobbyPlayingState);
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