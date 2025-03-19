
using UnityEngine;

namespace Terrix.Networking
{
    public class LobbySearchingState : LobbyState
    {
        private float Timer = 60f;
        public float TimeToStopTimer { get; private set; }

        public override void Enter()
        {
            TimeToStopTimer = Timer;
        }

        public override void Update()
        {
            TimeToStopTimer -= Time.deltaTime;
            if (TimeToStopTimer <= 0)
            {
                stateMachine.ChangeState(stateMachine.LobbyStartingState);
            }
        }

        public LobbySearchingState(LobbyStateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}