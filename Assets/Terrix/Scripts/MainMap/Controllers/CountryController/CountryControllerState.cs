using UnityEngine.InputSystem;

namespace Terrix.Controllers
{
    public partial class CountryController
    {
        private abstract class CountryControllerState
        {
            protected readonly CountryController CountryController;
            
            protected CountryControllerState(CountryController countryController)
            {
                this.CountryController = countryController;
            }

            public virtual void Enter() {}

            public virtual void OnDragBorders(InputAction.CallbackContext context) { }
            
            public virtual void OnChooseCountryPosition(InputAction.CallbackContext context) { }
            
            public virtual void OnPoint(InputAction.CallbackContext context) { }
            
            public virtual void Update() { }

            public virtual void Exit() { }
        }
    }
}