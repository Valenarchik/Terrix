namespace Terrix.Controllers
{

    public partial class CountryController
    {
        private class CountryControllerStateMachine
        {
            public CountryControllerState CurrentState { get; private set; }

            public void Initialize(CountryControllerState state)
            {
                CurrentState = state;
                state.Enter();
            }

            public void ChangeState(CountryControllerState newState)
            {
                CurrentState.Exit();
                CurrentState = newState;
                CurrentState.Enter();
            }
        }
    }
}