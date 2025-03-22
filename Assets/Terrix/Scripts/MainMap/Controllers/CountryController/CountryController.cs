using System;
using CustomUtilities.Attributes;
using Terrix.DTO;
using Terrix.Game.GameRules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Terrix.Controllers.Country
{
    public partial class CountryController : MonoBehaviour
    {
        [SerializeField] private new Camera camera;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private MainMapCameraController cameraController;
        [SerializeField] private MainMap mainMap;
        

        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private GamePhaseType currentPhase;
        [SerializeField, ReadOnlyInspector] private CountryControllerStateType controllerStateType;

        private CountryControllerStateMachine stateMachine;
        private IdleState idleState;
        private ChooseFirstCountryPositionState chooseFirstCountryPositionState;

        public void OnChooseCountryPosition(InputAction.CallbackContext context)
        {
            stateMachine.CurrentState.OnChooseCountryPosition(context, mainMap);
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            stateMachine.CurrentState.OnPoint(context);
        }

        public void OnDragBorders(InputAction.CallbackContext context)
        {
            stateMachine.CurrentState.OnDragBorders(context);
        }

        public void Init_OnServer()
        {
        }

        public void Init_OnClient()
        {
            stateMachine = new CountryControllerStateMachine();
            idleState = new IdleState(this, CountryControllerStateType.Idle);
            chooseFirstCountryPositionState =
                new ChooseFirstCountryPositionState(this, CountryControllerStateType.ChooseCountry);
            stateMachine.Initialize(idleState);

            mainMap.Events.OnGameReady(OnGameReady);
        }

        private void OnGameReady()
        {
            mainMap.PhaseManager.PhaseChanged += OnPhaseChanged;
            ActualizePhase(mainMap.PhaseManager.CurrentPhase);
        }

        private void OnPhaseChanged()
        {
            ActualizePhase(mainMap.PhaseManager.CurrentPhase);
        }

        private void ActualizePhase(GamePhaseType phaseType)
        {
            currentPhase = phaseType;
            Debug.Log(currentPhase);
            switch (phaseType)
            {
                case GamePhaseType.Uninitialized:
                    stateMachine.ChangeState(idleState);
                    break;
                case GamePhaseType.Initial:
                    stateMachine.ChangeState(chooseFirstCountryPositionState);
                    break;
                case GamePhaseType.Main:
                    //TODO
                    break;
                case GamePhaseType.Finish:
                    stateMachine.ChangeState(idleState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phaseType), phaseType, null);
            }
        }
    }
}