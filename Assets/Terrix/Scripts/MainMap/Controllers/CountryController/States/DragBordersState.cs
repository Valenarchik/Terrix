using System;
using Terrix.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terrix.Controllers
{
    public partial class CountryController
    {
        private class DragBordersState : CountryControllerState
        {
            private static readonly Vector3Int DefaultStartDragHexPosition = new(-1, -1, -1);

            private Vector2 pointPosition;

            private Hex[] dragHexes;
            private float attackPoints;
            private int? attackTarget;

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
                    var newDragHexes = CountryController.StretchBorders(startDragHexPosition, cellPosition,
                        out attackTarget, out attackPoints);
                    CountryController.UpdateDragZone(dragHexes, newDragHexes);
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
                if (CountryController.IsOurBorder(cellPosition))
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
                    Debug.Log("Started drag");
                    drag = true;
                }
                else
                {
                    Debug.Log("Not started drag");
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
                CountryController.UpdateDragZone(dragHexes, newDragHexes);
                CountryController.StartAttack(attackTarget, attackPoints, dragHexes);
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