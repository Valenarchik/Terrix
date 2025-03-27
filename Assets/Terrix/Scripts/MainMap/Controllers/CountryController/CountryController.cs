using System;
using CustomUtilities.Attributes;
using Terrix.DTO;
using Terrix.Game.GameRules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Terrix.Controllers
{
    // только на клиенте
    public partial class CountryController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private new Camera camera;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private MainMapCameraController cameraController;
        [SerializeField] private PlayerCommandsExecutor commandsExecutor;
        
        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private GamePhaseType currentPhase;
        [SerializeField, ReadOnlyInspector] private CountryControllerStateType controllerStateType;
        [SerializeField, ReadOnlyInspector] private int playerId = int.MinValue;

        private CountryControllerStateMachine stateMachine;
        private IdleState idleState;
        private ChooseFirstCountryPositionState chooseFirstCountryPositionState;

        private IPhaseManager phaseManager;
        private GameEvents gameEvents;


        public void Initialize(int playerId, IPhaseManager phaseManager, GameEvents gameEvents)
        {
            this.playerId = playerId;
            gameObject.name = $"{nameof(CountryController)}_{playerId}";

            this.phaseManager = phaseManager;
            this.gameEvents = gameEvents;
        }

        public void OnChooseCountryPosition(InputAction.CallbackContext context)
        {
            stateMachine.CurrentState.OnChooseCountryPosition(context);
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            stateMachine.CurrentState.OnPoint(context);
        }
        
        public void OnDragBorders(InputAction.CallbackContext context)
        {
            stateMachine.CurrentState.OnDragBorders(context);
        }

        private void Start()
        {
            stateMachine = new CountryControllerStateMachine();
            idleState = new IdleState(this, CountryControllerStateType.Idle);
            chooseFirstCountryPositionState = new ChooseFirstCountryPositionState(this, CountryControllerStateType.ChooseCountry);
            stateMachine.Initialize(idleState);
            
           gameEvents.OnGameReady(OnGameReady);
        }

        private void OnGameReady()
        {
            phaseManager.PhaseChanged += OnPhaseChanged;
            ActualizePhase(phaseManager.CurrentPhase);
        }

        private void OnPhaseChanged()
        {
            ActualizePhase(phaseManager.CurrentPhase);
        }

        private void ActualizePhase(GamePhaseType phaseType)
        {
            currentPhase = phaseType;
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
        
        private void TryChooseInitCountryPosition(Vector3Int pos)
        {
            // клиент проводит проверку
            if (commandsExecutor.CanChooseInitialCountryPosition(playerId, pos))
            {
                commandsExecutor.ChooseInitialCountryPosition(playerId, pos);
            }
        }
    }
}
