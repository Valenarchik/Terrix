
using UnityEngine;

namespace Terrix.Networking
{
    public class LobbyStartingState : LobbyState
    {
        private float Timer = 5f;
        public float TimeToStopTimer { get; private set; }

        public override void Enter()
        {
            TimeToStopTimer = Timer;
        }

        public LobbyStartingState(LobbyStateMachine stateMachine) : base(stateMachine)
        {
        }


        public override void Update()
        {
            TimeToStopTimer -= Time.deltaTime;
            if (TimeToStopTimer <= 0)
            {
                stateMachine.ChangeState(stateMachine.LobbyPlayingState);
            }
        }
    }
}