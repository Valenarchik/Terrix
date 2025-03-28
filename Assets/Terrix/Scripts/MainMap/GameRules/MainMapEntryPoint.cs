using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Terrix.Controllers;
using System.Linq;
using FishNet.Transporting;
using Terrix.Controllers.Country;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Networking;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Terrix.Game.GameRules
{
    // Нужно разбить на 2 класса, для сервера и для клиента
    public class MainMapEntryPoint : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private HexMapGenerator mapGenerator;

        [SerializeField] private TickGenerator tickGenerator;
        [SerializeField] private Lobby lobby;
        [SerializeField] private MainMapCameraController cameraController;
        [SerializeField] private AllCountriesHandler allCountriesHandler;
        [SerializeField] private AllCountriesDrawer allCountriesDrawer;
        [SerializeField] private CountryController countryController;
        [SerializeField] private PlayerCommandsExecutor commandsExecutor;

        private IGameDataProvider gameDataProvider;
        private IPlayersFactory playersFactory;
        private IGameRefereeFactory gameRefereeFactory;

        private GameEvents events;
        private HexMap map;
        private ICountriesCollector countriesCollector;
        private IGameReferee referee;
        private IAttackInvoker attackInvoker;
        private IPhaseManager phaseManager;
        private IPlayersProvider players;
        private Dictionary<NetworkConnection, int> playersIds = new();
        private Settings settings;

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManagerOnRemoteConnectionState_OnServer;
            var gameMode = GameModeType.FFA;
            var playersAndBots = new PlayersAndBots(lobby.PlayersMaxCount, 100);
            var countriesDrawerSettings = new AllCountriesDrawer.Settings(Enumerable.Range(0, lobby.PlayersMaxCount).Select(i => new ZoneData(i)).ToArray(),
                new ZoneData(AllCountriesDrawer.DRAG_ZONE_ID)
                {
                    Color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))
                });
            //Server settings
            settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(gameMode),
                playersAndBots,
                countriesDrawerSettings,
                0
            );
            Initialize_OnServer(settings);
            lobby.LobbyStateMachine.OnStateChanged += LobbyStateMachineOnStateChanged;
        }

        protected void ServerManagerOnRemoteConnectionState_OnServer(NetworkConnection conn,
            RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Stopped:
                    playersIds.Remove(conn);
                    break;
                case RemoteConnectionState.Started:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void LobbyStateMachineOnStateChanged(LobbyState state)
        {
            if (state == lobby.LobbyStateMachine.LobbyStartingState)
            {
                StartGamePipeline();
            }
        }

        public void StartGamePipeline(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            StartCoroutine(GamePipeline(serverSettings, clientSettings));
        }

        // Нужно так же делить на серверную и клиентскуюя часть
        public override void OnStartClient()
        {
            base.OnStartClient();
            var color = PlayerDataHolder.Color;
            Initialize_OnClient_ToServer(ClientManager.Connection, color);
        }

        private IEnumerator GamePipeline(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            //TODO тут инициализация происходит и для сервака и дял клиента
            InitializeAllCountriesDrawer_ToObserver(settings.CountryDrawerSettings);

            phaseManager.NextPhase();
            ChangePhase();
            var gameData = gameDataProvider.Get();
            BotsChooseRandomPositions();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
            NotInitializedPlayersChooseRandomPositions();

            phaseManager.NextPhase();
            tickGenerator.Initialize(new ITickHandler[]
            {
                countriesCollector,
                attackInvoker,
                referee
            });
        }

        [ObserversRpc]
        private void ChangePhase()
        {
            phaseManager.NextPhase();
        }

        private void Initialize_OnServer(Settings settings)
        // TODO private void Initialize(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);
            map = mapGenerator.GenerateMap(settings.MapSettings);
            // events = new GameEvents();
            // attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();

            playersFactory = new PlayersFactory(gameDataProvider);
            players = new PlayersProvider(playersFactory.CreatePlayers(serverSettings.PlayersCount));
            gameRefereeFactory = new GameRefereeFactory(serverSettings.GameModeSettings, players);
            referee = gameRefereeFactory.Create();
            countriesCollector = new CountriesCollector(players);

            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
        }

        [ServerRpc(RequireOwnership = false)]
        private void Initialize_OnClient_ToServer(NetworkConnection connection, Color color)
        {
            int id = -1;
            for (int i = 0; i < players.GetAll().Length; i++)
            {
                if (!playersIds.Values.Contains(i))
                {
                    id = i;
                    playersIds.Add(connection, id);
                    break;
                }
            }

            if (id == -1)
            {
                throw new Exception("Id wasn't set");
            }

            settings.CountryDrawerSettings.Zones[id].Color = color;

            Initialize_OnClient_ToTarget(connection, map, id);
        }


        [TargetRpc]
        private void Initialize_OnClient_ToTarget(NetworkConnection connection, HexMap map, int id)
        {
            settings = new Settings(id);
            this.map = map;
            mapGenerator.UpdateMap(map);
            gameDataProvider = new GameDataProvider();

            events = new GameEvents();
            attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();
            countryController.Initialize(settings.LocalPlayerId, phaseManager, events);
            cameraController.Initialize(events);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
            events.StartGame();
        }

        [ObserversRpc]
        private void InitializeAllCountriesDrawer_ToObserver(AllCountriesDrawer.Settings countriesDrawerSettings)
        {
            allCountriesDrawer.Initialize(countriesDrawerSettings);
        }

        // [Server]
        private void BotsChooseRandomPositions()
        {
            commandsExecutor.ChooseRandomInitialCountryPosition(players.GetAll().OfType<Bot>());
        }

        // [Server]
        private void NotInitializedPlayersChooseRandomPositions()
        {
            commandsExecutor.ChooseRandomInitialCountryPosition(players.GetAll()
                .Where(p => p.Country.TotalCellsCount == 0));
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20
            };

            GUI.Label(new Rect(0, 0, 300, 25), $"CurrentPhase is {phaseManager.CurrentPhase}", labelStyle);
            GUI.Label(new Rect(0, 25, 300, 25), $"Time is {Time.time:F2}", labelStyle);
        }
#endif


        public class ServerSettings
        {
            public HexMapGenerator.Settings MapSettings { get; }
            public GameReferee.Settings GameModeSettings { get; }
            public PlayersAndBots PlayersCount { get; }
            public AllCountriesDrawer.Settings CountryDrawerSettings { get; }

            public ServerSettings(
                HexMapGenerator.Settings mapSettings,
                GameReferee.Settings gameModeSettings,
                PlayersAndBots playersCount,
                AllCountriesDrawer.Settings countryDrawerSettings)
            {
                MapSettings = mapSettings ?? throw new NullReferenceException(
                    $"{nameof(MainMapEntryPoint)}.{nameof(ServerSettings)}.{nameof(MapSettings)}");
                GameModeSettings = gameModeSettings ?? throw new NullReferenceException(
                    $"{nameof(MainMapEntryPoint)}.{nameof(ServerSettings)}.{nameof(GameModeSettings)}");
                PlayersCount = playersCount;
                CountryDrawerSettings = countryDrawerSettings ?? throw new NullReferenceException(
                    $"{nameof(MainMapEntryPoint)}.{nameof(ServerSettings)}.{nameof(CountryDrawerSettings)}");
            }
        }

        public class ClientSettings
        {
            public int LocalPlayerId { get; }

            public ClientSettings(int localPlayerId)
            {
                LocalPlayerId = localPlayerId;
            }
        }
    }
}