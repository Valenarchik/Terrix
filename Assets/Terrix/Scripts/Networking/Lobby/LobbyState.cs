using UnityEngine;

namespace Terrix.Networking
{
    public class LobbyState
    {
        protected LobbyStateMachine stateMachine;
        public LobbyStateType LobbyStateType { get; protected set; }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void Update()
        {
        }

        public LobbyState(LobbyStateMachine stateMachine, LobbyStateType lobbyStateType)
        {
            this.stateMachine = stateMachine;
            LobbyStateType = lobbyStateType;
        }
    }

    public class LobbyTimerState : LobbyState
    {
        private float timer;
        public float TimeToStopTimer { get; private set; }
        private LobbyState nextState;

        public override void Enter()
        {
            TimeToStopTimer = timer;
        }

        public LobbyTimerState(LobbyStateMachine stateMachine, LobbyStateType stateType, float time) : base(stateMachine,
            stateType)
        {
            timer = time;
        }

        public void SetNextState(LobbyState state)
        {
            nextState = state;
        }


        public override void Update()
        {
            TimeToStopTimer -= Time.deltaTime;
            if (TimeToStopTimer <= 0)
            {
                stateMachine.ChangeState(nextState);
            }
        }
    }

    public enum LobbyStateType
    {
        Searching,
        BeforeStarting,
        Starting,
        Playing,
        Ended
    }
}