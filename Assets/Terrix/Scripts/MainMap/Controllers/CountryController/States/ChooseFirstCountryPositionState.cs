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

            public override void OnChooseCountryPosition(InputAction.CallbackContext context, MainMap mainMap)
            {
                if (context.phase == InputActionPhase.Performed)
                {
                    var cellPosition = MapUtilities.GetHexPosition(pointPosition,
                        CountryController.camera,
                        CountryController.tilemap);
                    var hex = mainMap.Map[cellPosition];
                }
            }

            public override void OnPoint(InputAction.CallbackContext context)
            {
                pointPosition = context.ReadValue<Vector2>();
            }
        }
    }
}