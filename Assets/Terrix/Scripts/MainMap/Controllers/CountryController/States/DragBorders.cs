using System;
using Terrix.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terrix.Controllers
{
    public partial class CountryController
    {
        private class DragBorders : CountryControllerState
        {
            private HexMap map => CountryController.map;
            private Vector2 pointPosition;

            private Hex[] dragHexes;
            private bool drag;
            private Vector3Int startDragHexPosition;

            public DragBorders(CountryController countryController) : base(countryController)
            {
            }

            public override void Enter()
            {
                dragHexes = Array.Empty<Hex>();
                drag = false;
                startDragHexPosition = new Vector3Int(-1, -1, -1);
            }

            public override void Exit()
            {
                dragHexes = null;
            }

            public override void OnDragBorders(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Performed:
                    {
                        TryStartDrag();
                        break;
                    }
                    case InputActionPhase.Canceled:
                    {
                        TryFinishDrag();
                        break;
                    }
                }
            }

            public override void OnPoint(InputAction.CallbackContext context)
            {
                pointPosition = context.ReadValue<Vector2>();
            }

            public override void Update()
            {
                if (drag)
                {
                    var cellPosition = CountryController.GetCellPosition(pointPosition);
                    if (CountryController.IsNotOur(cellPosition))
                    {
                        var newDragHexes = CountryController.StretchBorders(startDragHexPosition, cellPosition);
                        var updateData = CountryController.GetUpdateData(dragHexes, newDragHexes);
                        CountryController.countriesDrawer.UpdateDragZone(updateData);
                        dragHexes = newDragHexes;
                    }
                }
            }

            private void TryStartDrag()
            {
                var cellPosition = CountryController.GetCellPosition(pointPosition);
                if (CountryController.IsBorder(cellPosition))
                {
                    drag = true;
                    startDragHexPosition = cellPosition;
                    CountryController.cameraController.EnableDrag = false;
                }
            }

            private void TryFinishDrag()
            {
                if (drag)
                {
                    Debug.Log(dragHexes.Length);
                    drag = false;
                    var newDragHexes = Array.Empty<Hex>();
                    var updateData = CountryController.GetUpdateData(dragHexes, newDragHexes);
                    CountryController.countriesDrawer.UpdateDragZone(updateData);
                    dragHexes = newDragHexes;
                    CountryController.cameraController.EnableDrag = true;
                }
            }
        }
    }
}