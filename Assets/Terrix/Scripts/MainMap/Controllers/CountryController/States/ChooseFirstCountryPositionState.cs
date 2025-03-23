using Terrix.Game.GameRules;
using Terrix.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terrix.Controllers.Country
{
    public partial class CountryController
    {
        private class ChooseFirstCountryPositionState : CountryControllerState
        {
            private Vector2 pointPosition;

            public ChooseFirstCountryPositionState(CountryController countryController,
                CountryControllerStateType stateType) : base(countryController, stateType)
            {
            }

            public override void OnChooseCountryPosition(InputAction.CallbackContext context)
            {
                if (context.phase == InputActionPhase.Performed)
                {
                    var cellPosition = MapUtilities.GetMousePosition(pointPosition,
                        CountryController.camera,
                        CountryController.tilemap);
                    
                    Debug.Log(cellPosition);
                    
                    //(cellPosition.x, cellPosition.y) = (cellPosition.y, cellPosition.x);
                    CountryController.TryChooseInitCountryPosition(cellPosition);
                }
            }

            public override void OnPoint(InputAction.CallbackContext context)
            {
                pointPosition = context.ReadValue<Vector2>();
            }
        }
    }
}