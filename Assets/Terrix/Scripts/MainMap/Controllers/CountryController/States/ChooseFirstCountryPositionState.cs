using Terrix.Game.GameRules;
using Terrix.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terrix.Controllers
{
    public partial class CountryController
    {
        private class ChooseFirstCountryPositionState : CountryControllerState
        {
            private Vector2 pointPosition;

            public ChooseFirstCountryPositionState(CountryController countryController) : base(countryController)
            {
            }

            public override void OnChooseCountryPosition(InputAction.CallbackContext context)
            {
                if (context.phase == InputActionPhase.Performed)
                {
                    var cellPosition = CountryController.GetCellPosition(pointPosition);
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