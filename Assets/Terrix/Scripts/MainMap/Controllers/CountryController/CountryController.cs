using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Attributes;
using JetBrains.Annotations;
using Priority_Queue;
using FishNet.Object;
using Terrix.DTO;
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

        [Header("Stretch borders")]
        [SerializeField] private float alpha = 1.2f;

        [SerializeField] private float beta = 10f;

        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private GamePhaseType currentPhase;

        [SerializeField, ReadOnlyInspector] private CountryControllerStateType controllerStateType;
        [SerializeField, ReadOnlyInspector] private int playerId = int.MinValue;

        private CountryControllerStateMachine stateMachine;
        private Dictionary<CountryControllerStateType, CountryControllerState> states;

        private IPhaseManager phaseManager;
        private GameEvents gameEvents;
        private IPlayersProvider playersProvider;
        private HexMap map;
        private IGameDataProvider gameDataProvider;
        private Country country;


        public void Initialize(int playerId,
            [NotNull] IPhaseManager phaseManager,
            [NotNull] GameEvents gameEvents,
            [NotNull] IPlayersProvider playersProvider,
            [NotNull] HexMap map,
            [NotNull] IGameDataProvider gameDataProvider)
        {
            this.playerId = playerId;
            gameObject.name = $"{nameof(CountryController)}_{playerId}";

            this.phaseManager = phaseManager ?? throw new ArgumentNullException(nameof(phaseManager));
            this.gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(gameEvents));
            this.playersProvider = playersProvider ?? throw new ArgumentNullException(nameof(playersProvider));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            this.gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));

            this.country = this.playersProvider.Find(playerId).Country;
            this.gameEvents.OnGameReady(OnGameReady);
        }

        public void OnChooseCountryPosition(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnChooseCountryPosition(context);
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnPoint(context); //Ошибка
        }

        public void OnDragBorders(InputAction.CallbackContext context)
        {
            stateMachine?.CurrentState.OnDragBorders(context);
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
            // клиент проводит проверку
            if (await commandsExecutor.CanChooseInitialCountryPosition_OnClient(playerId, pos))
            {
                commandsExecutor.ChooseInitialCountryPosition(playerId, pos);
            }
        }

        private bool IsBorder(Vector3Int pos)
        {
            return map.HasHex(pos) && map[pos].GetNeighbours(map).Any(hex => hex.PlayerId == null);
        }

        private bool IsNotOur(Vector3Int pos)
        {
            return map.HasHex(pos) && map[pos].PlayerId != playerId;
        }

        private Country.UpdateCellsData GetUpdateData(Hex[] previousDragZoneHexes, Hex[] currentDragZoneHexes)
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

            return new Country.UpdateCellsData(AllCountriesDrawer.DRAG_ZONE_ID, changeData.ToArray());
        }
        //TODO метод для дебага. Нужно будет потом убрать
        private Country.UpdateCellsData GetUpdateDataActual(Hex[] previousDragZoneHexes, Hex[] currentDragZoneHexes)
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

            return new Country.UpdateCellsData(country.PlayerId, changeData.ToArray());
        }

        private Hex[] StretchBorders(
            Vector3Int startPos,
            Vector3Int endPos)
        {
            var gameData = gameDataProvider.Get();

            var start = map[startPos];
            var end = map[endPos];

            var direction = tilemap.CellToWorld(end.Position) - tilemap.CellToWorld(start.Position);
            var result = new List<Hex>();
            var visited = new HashSet<Hex>();
            var priorityQueue = new SimplePriorityQueue<Hex, float>();

            foreach (var neighbour in start.GetNeighbours(map))
            {
                if (!country.Contains(neighbour) || !neighbour.GetHexData(gameData).CanCapture)
                {
                    priorityQueue.Enqueue(neighbour, 0);
                }
            }

            var remainingPoints = country.Population;
            var targetReached = false;

            while (priorityQueue.Count > 0 && remainingPoints > 0 && !targetReached)
            {
                priorityQueue.TryDequeue(out var cell);
                if (visited.Contains(cell) || country.Contains(cell))
                {
                    continue;
                }

                var cellCost = cell.GetCost(playersProvider, gameData);
                if (cellCost > remainingPoints)
                {
                    continue;
                }

                result.Add(cell);

                if (end.Equals(cell))
                {
                    targetReached = true;
                }

                remainingPoints -= cellCost;
                visited.Add(cell);


                foreach (var neighbour in cell.GetNeighbours(map))
                {
                    if (country.Contains(neighbour) || visited.Contains(neighbour) ||
                        !neighbour.GetHexData(gameData).CanCapture)
                    {
                        continue;
                    }

                    var priority = CalculatePriority(neighbour);
                    priorityQueue.Enqueue(neighbour, -priority);
                }
            }

            return result.ToArray();

            float CalculatePriority(Hex current)
            {
                var delta = tilemap.CellToWorld(current.Position) - tilemap.CellToWorld(start.Position);

                var projection = Vector3.Dot(delta.normalized, direction.normalized);

                var distance = delta.magnitude;
                var distanceFactor = 1f / (1f + distance);

                return projection * alpha + distanceFactor * beta;
            }
        }

        private Vector3Int GetCellPosition(Vector2 pointPos)
        {
            return MapUtilities.GetMousePosition(pointPos, camera, tilemap);
        }

        [ObserversRpc]
        public void UpdateCountries_ToObserver(Dictionary<int, Country> countries)
        {
            foreach (var currentPlayer in playersProvider.GetAll())
            {
                currentPlayer.Country = countries[currentPlayer.ID];
                currentPlayer.Country.Owner = currentPlayer;
                // .First(currentCountry => currentCountry.PlayerId == currentPlayer.ID);
            }

            country = playersProvider.Find(playerId).Country;
        }

        public void UpdateCountries_OnClient(IPlayersProvider playersProvider)
        {
            this.playersProvider = playersProvider;
            country = playersProvider.Find(playerId).Country;
        }
    }
}