namespace Terrix.Networking
{
    public abstract class LobbyState
    {
        protected LobbyStateMachine stateMachine;

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void Update()
        {
        }

        public LobbyState(LobbyStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }
}