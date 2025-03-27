namespace Terrix.Controllers
{
    public partial class CountryController
    {
        /// <summary>
        /// Ничего не делать, работает только камера
        /// </summary>
        private class IdleState : CountryControllerState
        {
            public IdleState(CountryController countryController, CountryControllerStateType stateType) :
                base(countryController, stateType)
            {
            }

            public override void Enter()
            {
                CountryController.cameraController.EnableZoom = true;
                CountryController.cameraController.EnableDrag = true;
                CountryController.cameraController.EnableMove = true;
                base.Enter();
            }
        }
    }
}