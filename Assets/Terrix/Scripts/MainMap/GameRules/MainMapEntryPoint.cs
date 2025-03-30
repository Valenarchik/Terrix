using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Terrix.Controllers;
using System.Linq;
using FishNet.Transporting;
using MoreLinq;
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
        // private Settings settings;
        private ServerSettings serverSettings;
        private ClientSettings clientSettings;

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManagerOnRemoteConnectionState_OnServer;
            var gameMode = GameModeType.FFA;
            var playersAndBots = new PlayersAndBots(lobby.PlayersMaxCount, 0);
            //TODO исправить
            var countriesDrawerSettings = new AllCountriesDrawer.Settings(
                Enumerable.Range(0, lobby.PlayersMaxCount).Select(i => new ZoneData(i)).ToArray(),
                new ZoneData(AllCountriesDrawer.DRAG_ZONE_ID)
                {
                    Color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))
                });
            //Server settings
            serverSettings = new ServerSettings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(gameMode),
                playersAndBots,
                countriesDrawerSettings
            );
            Initialize_OnServer(serverSettings);
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

        public void StartGamePipeline()
        {
            StartCoroutine(GamePipeline());
        }

        // Нужно так же делить на серверную и клиентскуюя часть
        public override void OnStartClient()
        {
            base.OnStartClient();
            var color = PlayerDataHolder.Color;
            Initialize_OnClient_ToServer(ClientManager.Connection, color);
        }

        private IEnumerator GamePipeline()
        {
            //TODO тут инициализация происходит и для сервака и дял клиента
            Initialize_InitialPhase_ToObserver(serverSettings.CountryDrawerSettings, players);

            phaseManager.NextPhase();
            ChangePhase();
            var gameData = gameDataProvider.Get();
            BotsChooseRandomPositions();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
            NotInitializedPlayersChooseRandomPositions();
            phaseManager.NextPhase();
            ChangePhase();
            tickGenerator.Initialize(new ITickHandler[]
            {
                countriesCollector,
                attackInvoker,
                referee
            });
            countryController.UpdateCountries_OnClient(
                allCountriesHandler.Countries.ToDictionary(country => country.PlayerId));
        }

        [ObserversRpc]
        private void ChangePhase()
        {
            phaseManager.NextPhase();
        }

        private void Initialize_OnServer(ServerSettings settings)
            // TODO private void Initialize(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            gameDataProvider = new GameDataProvider();
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

            serverSettings.CountryDrawerSettings.Zones[id].Color = color;

            Initialize_OnClient_ToTarget(connection, map, id);
        }


        [TargetRpc]
        private void Initialize_OnClient_ToTarget(NetworkConnection connection, HexMap map, int id)
        {
            clientSettings = new ClientSettings(id);
            this.map = map;
            mapGenerator.UpdateMap(map);
            gameDataProvider = new GameDataProvider();

            events = new GameEvents();
            attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();
            // TODO Инициализировать когда все игроки подсодинятся
            // countryController.Initialize(clientSettings.LocalPlayerId, phaseManager, events, players, this.map,
            //     gameDataProvider);
            cameraController.Initialize(events);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
            events.StartGame();
        }

        [ObserversRpc]
        private void Initialize_InitialPhase_ToObserver(AllCountriesDrawer.Settings countriesDrawerSettings,
            IPlayersProvider iPlayersProvider)
        {
            iPlayersProvider.GetAll().ForEach(player => player.Country.Owner = player);
            InitializeCountryController_OnClient(iPlayersProvider);
            InitializeAllCountriesDrawer_OnClient(countriesDrawerSettings);
        }

        private void InitializeAllCountriesDrawer_OnClient(AllCountriesDrawer.Settings countriesDrawerSettings)
        {
            allCountriesDrawer.Initialize(countriesDrawerSettings);
        }

        private void InitializeCountryController_OnClient(IPlayersProvider iPlayersProvider)
        {
            players = iPlayersProvider;
            countryController.Initialize(clientSettings.LocalPlayerId, phaseManager, events, players, map,
                gameDataProvider);
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

            // GUI.Label(new Rect(0, 0, 300, 25), $"CurrentPhase is {phaseManager.CurrentPhase}", labelStyle);
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