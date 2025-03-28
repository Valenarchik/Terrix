namespace Terrix.Controllers
{
    public partial class CountryController
    {
        /// <summary>
        /// Ничего не делать, работает только камера
        /// </summary>
        private class IdleState : CountryControllerState
        {
            public IdleState(CountryController countryController) : base(countryController)
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