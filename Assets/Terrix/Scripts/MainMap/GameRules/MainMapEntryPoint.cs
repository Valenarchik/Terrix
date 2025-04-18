using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Terrix.Controllers;
using System.Linq;
using FishNet.Transporting;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Network.DTO;
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
        [SerializeField] private LeaderboardUI leaderboardUI;

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

        private IAttackMassageEncoder attackMassageEncoder;

        private void TickGenerator_OnUpdated()
        {
            UpdatePlayersInfo_ToObserver(new NetworkSerialization.PlayersCountryMapData(players, map));
        }

        [ObserversRpc]
        private void UpdatePlayersInfo_ToObserver(NetworkSerialization.PlayersCountryMapData playersCountryMapData)
        {
            players = playersCountryMapData.IPlayersProvider;
            map = playersCountryMapData.HexMap;
            countryController.UpdateData_OnClient(playersCountryMapData);
            allCountriesDrawer.UpdateScore_OnClient(playersCountryMapData);
            leaderboardUI.UpdateInfo(players);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManagerOnRemoteConnectionState_OnServer;
            var gameMode = GameModeType.FFA;
            var playersAndBots = new PlayersAndBots(lobby.PlayersMaxCount,
                lobby.PlayersAndBotsMaxCount - lobby.PlayersMaxCount);
            // 0); // без ботов
            var countriesDrawerSettings = new AllCountriesDrawer.Settings(
                Enumerable.Range(0, lobby.PlayersAndBotsMaxCount).Select(i => new ZoneData(i)).ToArray(),
                new ZoneData(AllCountriesDrawer.DRAG_ZONE_ID));
            serverSettings = new ServerSettings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(gameMode),
                playersAndBots,
                countriesDrawerSettings
            );
            Initialize_OnServer(serverSettings);
            lobby.LobbyStateMachine.OnStateChanged += LobbyStateMachineOnStateChanged;
            tickGenerator.OnUpdated += TickGenerator_OnUpdated;
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
            var playerName = PlayerDataHolder.PlayerName;
            Initialize_OnClient_ToServer(ClientManager.Connection, color, playerName);
        }

        private IEnumerator GamePipeline()
        {
            //TODO тут инициализация происходит и для сервака и дял клиента
            foreach (var bot in players.GetAll().Where(player => player.PlayerType is PlayerType.Bot))
            {
                bot.PlayerName = $"Bot {bot.ID}";
                bot.PlayerColor = new Color(Random.Range(0, 1f),
                    Random.Range(0, 1f), Random.Range(0, 1f), 1f);
                serverSettings.CountryDrawerSettings.Zones[bot.ID].Color = bot.PlayerColor;
                serverSettings.CountryDrawerSettings.Zones[bot.ID].PlayerName = bot.PlayerName;
            }

            Initialize_InitialPhase_ToObserver(serverSettings.CountryDrawerSettings,
                new NetworkSerialization.PlayersCountryMapData(players, map));

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
            countryController.UpdateCountries_ToObserver(
                allCountriesHandler.Countries.ToDictionary(country => country.PlayerId));
        }

        [ObserversRpc]
        private void ChangePhase()
        {
            phaseManager.NextPhase();
        }

        private void Initialize_OnServer(ServerSettings settings)
        {
            gameDataProvider = new GameDataProvider();
            //TODO Страрая карта
            // map = mapGenerator.GenerateMap(settings.MapSettings);

            // events = new GameEvents();
            // attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();
            playersFactory = new PlayersFactory(gameDataProvider);
            players = new PlayersProvider(playersFactory.CreatePlayers(serverSettings.PlayersCount));
            mapGenerator.Initialize(gameDataProvider, players);
            map = mapGenerator.GenerateMap(serverSettings.MapSettings);
            // countryController.Initialize(clientSettings.LocalPlayerId, phaseManager, events, players, map,
            //     gameDataProvider);

            attackInvoker = new AttackInvoker();

            gameRefereeFactory = new GameRefereeFactory(serverSettings.GameModeSettings, players);
            referee = gameRefereeFactory.Create();
            countriesCollector = new CountriesCollector(players);

            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());

            attackMassageEncoder = new AttackMassageEncoder(players, map);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider, attackMassageEncoder,
                attackInvoker);
        }

        [ServerRpc(RequireOwnership = false)]
        private void Initialize_OnClient_ToServer(NetworkConnection connection, Color color, string playerName)
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

            if (playerName == "")
            {
                playerName = $"Player {id}";
            }

            var player = players.Find(id);
            player.PlayerName = playerName;
            player.PlayerColor = color;


            serverSettings.CountryDrawerSettings.Zones[id].Color = color;
            serverSettings.CountryDrawerSettings.Zones[id].PlayerName = playerName;

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
            phaseManager = new PhaseManager();
            cameraController.Initialize(events);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider,
                new AttackMassageEncoder(players, map), new AttackInvoker());
            events.StartGame();
        }

        [ObserversRpc]
        private void Initialize_InitialPhase_ToObserver(AllCountriesDrawer.Settings countriesDrawerSettings,
            NetworkSerialization.PlayersCountryMapData playersCountryMapData)
        {
            var iPlayersProvider = playersCountryMapData.IPlayersProvider;
            players = iPlayersProvider;
            var playerColor = countriesDrawerSettings.Zones[clientSettings.LocalPlayerId].Color;
            if (playerColor != null)
            {
                playerColor = new Color(playerColor.Value.r, playerColor.Value.g, playerColor.Value.b, 0.4f);
            }

            countriesDrawerSettings.DragZone.Color = playerColor;
            countriesDrawerSettings.DragZone.PlayerName = "";
            InitializeCountryController_OnClient();
            InitializeAllCountriesDrawer_OnClient(countriesDrawerSettings);
            leaderboardUI.Initialize(players);
        }

        private void InitializeAllCountriesDrawer_OnClient(AllCountriesDrawer.Settings countriesDrawerSettings)
        {
            allCountriesDrawer.Initialize(countriesDrawerSettings);
        }

        private void InitializeCountryController_OnClient()
        {
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