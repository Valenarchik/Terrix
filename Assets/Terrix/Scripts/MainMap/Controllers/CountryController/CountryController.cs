using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Attributes;
using JetBrains.Annotations;
using FishNet.Object;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Terrix.Controllers
{
    // только на клиенте
    public partial class CountryController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private new Camera camera;

        [SerializeField] private Tilemap tilemap;
        [SerializeField] private MainMapCameraController cameraController;
        [SerializeField] private PlayerCommandsExecutor commandsExecutor;
        [SerializeField] private AllCountriesDrawer countriesDrawer;
        [SerializeField] private BorderStretcher borderStretcher;
        
        [Header("Input")]
        [SerializeField, Range(0f, 1f)] private float fastAttackPopulationPercent = 0.2f;
        [SerializeField] private float attackBuffer = 0.01f;

        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private GamePhaseType currentPhase;

        [SerializeField, ReadOnlyInspector] private CountryControllerStateType controllerStateType;
        [SerializeField, ReadOnlyInspector] private int playerId = int.MinValue;

        private CountryControllerStateMachine stateMachine;
        private Dictionary<CountryControllerStateType, CountryControllerState> states;

        private IPhaseManager phaseManager;
        private IGame game;
        private IPlayersProvider players;
        private HexMap map;
        private IGameDataProvider gameDataProvider;
        private Player player;
        private Country country;


        public void Initialize(int playerId,
            [NotNull] IPhaseManager phaseManager,
            [NotNull] IGame game,
            [NotNull] IPlayersProvider playersProvider,
            [NotNull] HexMap map,
            [NotNull] IGameDataProvider gameDataProvider)
        {
            this.playerId = playerId;
            gameObject.name = $"{nameof(CountryController)}_{playerId}";

            this.phaseManager = phaseManager ?? throw new ArgumentNullException(nameof(phaseManager));
            this.game = game ?? throw new ArgumentNullException(nameof(game));
            this.players = playersProvider ?? throw new ArgumentNullException(nameof(playersProvider));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            this.gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));

            this.player = this.players.Find(playerId);
            this.country = this.player.Country;
            this.game.OnGameStarted(OnGameStart);
        }

        public void OnChooseCountryPosition(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnChooseCountryPosition(context);
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnPoint(context);
        }

        public void OnDragBorders(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnDragBorders(context);
        }
        
        public void OnFastAttack(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnFastAttack(context);
        }

        public override void OnStartClient()
        {
            stateMachine = new CountryControllerStateMachine();
            states = new Dictionary<CountryControllerStateType, CountryControllerState>()
            {
                [CountryControllerStateType.Idle] = new IdleState(this),
                [CountryControllerStateType.ChooseCountry] = new ChooseFirstCountryPositionState(this),
                [CountryControllerStateType.DragBorders] = new DragBordersState(this)
            };
            stateMachine.Initialize(states[CountryControllerStateType.Idle]);
        }

        private void Update()
        {
            stateMachine?.CurrentState.Update();
        }

        private void OnGameStart()
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
                    ChangeState(CountryControllerStateType.Idle);
                    break;
                case GamePhaseType.Initial:
                    ChangeState(CountryControllerStateType.ChooseCountry);
                    break;
                case GamePhaseType.Main:
                    ChangeState(CountryControllerStateType.DragBorders);
                    break;
                case GamePhaseType.Finish:
                    ChangeState(CountryControllerStateType.Idle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phaseType), phaseType, null);
            }
        }

        private void ChangeState(CountryControllerStateType stateType)
        {
            controllerStateType = stateType;
            stateMachine.ChangeState(states[stateType]);
        }

        private async void TryChooseInitCountryPosition(Vector3Int pos)
        {
            if (await commandsExecutor.CanChooseInitialCountryPosition_OnClient(playerId, pos))
            {
                commandsExecutor.ChooseInitialCountryPosition_ToServer(playerId, pos);
            }
        }

        private bool IsOurBorder(Vector3Int pos)
        {
            return map.TryGetHex(pos, out var hex) && country.GetInnerBorder().Contains(hex);
        }

        private bool IsNotOur(Vector3Int pos)
        {
            return map.TryGetHex(pos, out var hex) && hex.PlayerId != playerId;
        }

        private void UpdateDragZone(Hex[] previousDragZoneHexes, Hex[] currentDragZoneHexes)
        {
            var changeData = new List<Country.CellChangeData>();

            foreach (var removedHex in previousDragZoneHexes.Except(currentDragZoneHexes))
            {
                changeData.Add(new Country.CellChangeData(removedHex, Country.UpdateCellMode.Remove));
            }

            foreach (var addedHex in currentDragZoneHexes.Except(previousDragZoneHexes))
            {
                changeData.Add(new Country.CellChangeData(addedHex, Country.UpdateCellMode.Add));
            }

            var data = new Country.UpdateCellsData(AllCountriesDrawer.DRAG_ZONE_ID, changeData.ToArray());
            countriesDrawer.UpdateDragZone(data, country.Population);
        }

        private Hex[] StretchBorders(Vector3Int startPos, Vector3Int endPos, out int? attackTarget,
            out float attackPoints)
        {
            return borderStretcher.StretchBorders(startPos,
                endPos,
                map,
                country,
                out attackTarget,
                out attackPoints);
        }

        private Vector3Int GetCellPosition(Vector2 pointPos)
        {
            return MapUtilities.GetMousePosition(pointPos, camera, tilemap);
        }

        private void StartAttack(int? targetId, float points, IEnumerable<Hex> territory)
        {
            var target = targetId.HasValue ? players.Find(targetId.Value) : null;
            var attack = new AttackBuilder
            {
                Owner = player,
                Target = target,
                Points = Mathf.Clamp(points * (1 + attackBuffer), 0, country.Population),
                Territory = territory.ToHashSet()
            }.Build();

            commandsExecutor.ExecuteAttack(attack);
        }
        
        private void StartFastAttack(int? targetId)
        {
            var target = targetId.HasValue ? players.Find(targetId.Value) : null;
            
            var attack = new AttackBuilder
            {
                Owner = player,
                Target = target,
                Points = GetPercentOfPopulation(fastAttackPopulationPercent),
                IsGlobalAttack = true
            }.Build();
            
            commandsExecutor.ExecuteAttack(attack);
        }

        private float GetPercentOfPopulation(float percent)
        {
            percent = Math.Clamp(percent, 0f, 1f);
            return country.Population * percent;
        }
    }
}