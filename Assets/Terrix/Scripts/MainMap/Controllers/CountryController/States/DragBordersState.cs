using System;
using Terrix.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terrix.Controllers
{
    public partial class CountryController
    {
        private class DragBordersState: CountryControllerState
        {
            private static readonly Vector3Int DefaultStartDragHexPosition = new(-1, -1, -1);
            
            private Vector2 pointPosition;
            
            private Hex[] dragHexes;
            private bool drag;
            private Vector3Int startDragHexPosition;

            public DragBordersState(CountryController countryController) : base(countryController)
            {
            }

            public override void Enter()
            {
                dragHexes = Array.Empty<Hex>();
                drag = false;
                startDragHexPosition = DefaultStartDragHexPosition;
            }

            public override void Exit()
            {
                FinishDrag();
            }

            public override void OnDragBorders(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                    {
                        SetStartDragHexPosition();
                        break;
                    }
                    case InputActionPhase.Performed:
                    {
                        StartDrag();
                        break;
                    }
                    case InputActionPhase.Canceled:
                    {
                        FinishDrag();
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
                    Drag();
                }
            }

            private void Drag()
            {
                var cellPosition = GetCellPosition();
                if (CountryController.IsNotOur(cellPosition))
                {
                    var newDragHexes = CountryController.StretchBorders(startDragHexPosition, cellPosition);
                    var updateData = CountryController.GetUpdateData(dragHexes, newDragHexes);
                    CountryController.countriesDrawer.UpdateDragZone(updateData);
                    dragHexes = newDragHexes;
                }
            }

            private void SetStartDragHexPosition()
            {
                if (drag)
                {
                    return;
                }

                var cellPosition = GetCellPosition();
                if (CountryController.IsBorder(cellPosition))
                {
                    startDragHexPosition = cellPosition;
                    CountryController.cameraController.EnableDrag = false;
                }
            }

            private void StartDrag()
            {
                if (drag)
                {
                    return;
                }

                var cellPosition = GetCellPosition();
                if (cellPosition == startDragHexPosition)
                {
                    drag = true;
                }
            }

            private void FinishDrag()
            {
                if (!drag)
                {
                    return;
                }

                drag = false;
                var newDragHexes = Array.Empty<Hex>();
                var updateData = CountryController.GetUpdateData(dragHexes, newDragHexes);
                CountryController.countriesDrawer.UpdateDragZone(updateData);
                dragHexes = newDragHexes;
                CountryController.cameraController.EnableDrag = true;
                startDragHexPosition = DefaultStartDragHexPosition;
            }

            private Vector3Int GetCellPosition()
            {
                return CountryController.GetCellPosition(pointPosition);
            }
        }
    }
}