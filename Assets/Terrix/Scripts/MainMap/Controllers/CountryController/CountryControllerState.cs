using Terrix.Game.GameRules;
using UnityEngine.InputSystem;

namespace Terrix.Controllers.Country
{
    public partial class CountryController
    {
        private abstract class CountryControllerState
        {
            protected readonly CountryController CountryController;
            public CountryControllerStateType StateType { get; }
            
            protected CountryControllerState(CountryController countryController, CountryControllerStateType stateType)
            {
                this.CountryController = countryController;
                this.StateType = stateType;
            }

            public virtual void Enter()
            {
                CountryController.controllerStateType = StateType;
            }

            public virtual void OnDragBorders(InputAction.CallbackContext context) { }
            
            public virtual void OnChooseCountryPosition(InputAction.CallbackContext context, MainMap map) { }
            
            public virtual void OnPoint(InputAction.CallbackContext context) { }
            
            public virtual void Update() { }

            public virtual void Exit() { }
        }
    }
}