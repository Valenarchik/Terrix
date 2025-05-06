using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Terrix.Controllers;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.MainMap.AI;
using Terrix.Map;
using Terrix.Network.DTO;
using Terrix.Networking;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Terrix.Game.GameRules
{
    // Нужно разбить на 2 класса, для сервера и для клиента
    public class MainMapEntryPoint : NetworkBehaviour
    {
        [Header("MapReferences")]
        [SerializeField] private HexMapGenerator mapGenerator;
        [SerializeField] private HexMapRenderer mapRenderer;
        [SerializeField] private HexMapGeneratorSettingsSO defaultMapGenerationSettings;

        [Header("References")]
        [SerializeField] private TickGenerator tickGenerator;
        [SerializeField] private Lobby lobby;
        [SerializeField] private MainMapCameraController cameraController;
        [SerializeField] private AllCountriesHandler allCountriesHandler;
        [SerializeField] private AllCountriesDrawer allCountriesDrawer;
        [SerializeField] private CountryController countryController;
        [SerializeField] private PlayerCommandsExecutor commandsExecutor;
        [SerializeField] private GameUI gameUI;
        // [SerializeField] private LeaderboardUI leaderboardUI;
        [SerializeField] private LobbyUI lobbyUI;


        private IGameDataProvider gameDataProvider;
        private IPlayersFactory playersFactory;
        private IGameRefereeFactory gameRefereeFactory;

        private IGame game;
        public HexMap Map { get; private set; }
        private ICountriesCollector countriesCollector;
        private IGameReferee referee;
        private AttackInvoker attackInvoker;
        private IPhaseManager phaseManager;
        private IPlayersProvider players;
        private Dictionary<NetworkConnection, int> playersIds = new();
        private ServerSettings serverSettings;
        private ClientSettings clientSettings;

        private IAttackMassageEncoder attackMassageEncoder;
        private IBotsManager botsManager;

        public override void OnStartServer()
        {
            base.OnStartServer();
            // NetworkManager.ServerManager.OnRemoteConnectionState += ServerManagerOnRemoteConnectionState_OnServer;
            var gameMode = GameModeType.FFA;
            var playersAndBots = new PlayersAndBots(lobby.PlayersMaxCount,
                lobby.PlayersAndBotsMaxCount - lobby.PlayersMaxCount);
            // 0); // без ботов
            var countriesDrawerSettings = new AllCountriesDrawer.Settings(
                Enumerable.Range(0, lobby.PlayersAndBotsMaxCount).Select(i => new ZoneData(i)).ToArray(),
                new ZoneData(AllCountriesDrawer.DRAG_ZONE_ID));
            serverSettings = new ServerSettings(
                defaultMapGenerationSettings.Get(),
                new GameReferee.Settings(GameModeType.FFA, 0.05f),
                playersAndBots,
                countriesDrawerSettings
            );
            Initialize_OnServer(serverSettings);
            lobby.LobbyStateMachine.OnStateChanged += LobbyStateMachineOnStateChanged;
            lobby.OnPlayerExit += LobbyOnPlayerExit;
        }

        private void LobbyOnPlayerExit(NetworkConnection conn)
        {
            playersIds.Remove(conn);
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
                serverSettings.CountryDrawerSettings.Zones[bot.ID].BorderColor = bot.PlayerColor;
                serverSettings.CountryDrawerSettings.Zones[bot.ID].PlayerName = bot.PlayerName;
            }

            Player.OnGameEnd += PlayerOnGameEnd_OnServer;

            // game = new Game();
            foreach (var player in players.GetAll())
            {
                Debug.Log($"{player.PlayerName} {player.Country.Population}");
            }

            Initialize_InitialPhase_ToObserver(serverSettings.CountryDrawerSettings,
                new NetworkSerialization.PlayersCountryMapData(players, Map));

            phaseManager.NextPhase();
            ChangePhase();

            var gameData = gameDataProvider.Get();
            BotsChooseRandomPositions();
            yield return new WaitForSeconds((float)gameData.TimeForChooseFirstCountryPosition.TotalSeconds);
            NotInitializedPlayersChooseRandomPositions();
            phaseManager.NextPhase();
            ChangePhase();

            botsManager.AddBots(players.GetAll().OfType<Bot>());
            tickGenerator.InitializeLoop(new TickGenerator.TickHandlerTuple[]
            {
                new(countriesCollector, gameData.TickHandlers[TickHandlerType.CountriesCollectorHandler]),
                new(attackInvoker, gameData.TickHandlers[TickHandlerType.AttackHandler]),
                new(referee, gameData.TickHandlers[TickHandlerType.RefereeHandler]),
                new(botsManager, gameData.TickHandlers[TickHandlerType.BotsHandler])
            });
            // Ждем пока игра не закончится
            yield return new WaitWhile(() => !game.GameOver);
            Debug.Log("Game over");
            phaseManager.NextPhase();
            ChangePhase();

            tickGenerator.StopLoop();
        }


        [ObserversRpc]
        private void ChangePhase()
        {
            phaseManager.NextPhase();
        }

        private void Initialize_OnServer(ServerSettings settings)
        {
            gameDataProvider = new GameDataProvider();
            phaseManager = new PhaseManager();
            playersFactory = new PlayersFactory(gameDataProvider);
            players = new PlayersProvider(playersFactory.CreatePlayers(serverSettings.PlayersCount));
            mapGenerator.Initialize(gameDataProvider, players);
            Map = mapGenerator.GenerateMap(serverSettings.MapSettings);
            attackInvoker = new AttackInvoker();
            game = new Game();
            gameRefereeFactory = new GameRefereeFactory(serverSettings.GameModeSettings, players, game);
            referee = gameRefereeFactory.Create();
            countriesCollector = new CountriesCollector(players);

            allCountriesHandler.Initialize_OnServer(players);

            attackMassageEncoder = new AttackMassageEncoder(players, Map);
            commandsExecutor.Initialize(Map, phaseManager, players, gameDataProvider, attackMassageEncoder,
                attackInvoker);
            botsManager = new BotsManager(attackInvoker, gameDataProvider, players);
        }

        [ServerRpc(RequireOwnership = false)]
        private void Initialize_OnClient_ToServer(NetworkConnection connection, Color color, string playerName)
        {
            int id = -1;
            for (int i = 0; i < players.GetPlayersCount(); i++)
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

            var mainColor = color;
            color.a = 0.3f;
            serverSettings.CountryDrawerSettings.Zones[id].Color = mainColor;
            serverSettings.CountryDrawerSettings.Zones[id].BorderColor = color;
            serverSettings.CountryDrawerSettings.Zones[id].PlayerName = playerName;

            Initialize_OnClient_ToTarget(connection, Map, id);
        }


        [TargetRpc]
        private void Initialize_OnClient_ToTarget(NetworkConnection connection, HexMap map, int id)
        {
            clientSettings = new ClientSettings(id);
            this.Map = map;
            mapRenderer.RenderMap(map);
            gameDataProvider = new GameDataProvider();

            game = new Game();
            phaseManager = new PhaseManager();
            //TODO Костыль
            cameraController.Initialize(game, phaseManager, null);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider,
                new AttackMassageEncoder(players, map), new AttackInvoker());
            game.StartGame();
        }

        [ObserversRpc]
        private void Initialize_InitialPhase_ToObserver(AllCountriesDrawer.Settings countriesDrawerSettings,
            NetworkSerialization.PlayersCountryMapData playersCountryMapData)
        {
            players = playersCountryMapData.IPlayersProvider;
            Map = playersCountryMapData.HexMap;
            var player = players.Find(clientSettings.LocalPlayerId);
            SetDragZoneInfo_OnClient(countriesDrawerSettings);
            allCountriesHandler.Initialize_OnClient(players, Map);
            InitializeCountryController_OnClient();
            InitializeAllCountriesDrawer_OnClient(countriesDrawerSettings);
            gameUI.Initialize(players, player);
        }

        private void SetDragZoneInfo_OnClient(AllCountriesDrawer.Settings countriesDrawerSettings)
        {
            var playerColor = countriesDrawerSettings.Zones[clientSettings.LocalPlayerId].Color;
            var borderColor = countriesDrawerSettings.Zones[clientSettings.LocalPlayerId].BorderColor;
            countriesDrawerSettings.DragZone.Color = playerColor;
            countriesDrawerSettings.DragZone.BorderColor = borderColor;
            countriesDrawerSettings.DragZone.PlayerName = "";
        }

        private void PlayerOnGameEnd_OnServer(int id, bool win)
        {
            PlayerOnGameEnd_ToTarget(id, win);
        }

        [ObserversRpc]
        private void PlayerOnGameEnd_ToTarget(int id, bool win)
        {
            if (win)
            {
                players.Find(id).Win();
                if (clientSettings.LocalPlayerId == id)
                {
                    gameUI.WinGame();
                }
            }
            else
            {
                players.Find(id).Lose();
                if (clientSettings.LocalPlayerId == id)
                {
                    gameUI.LoseGame();
                }
            }
        }

        private void InitializeAllCountriesDrawer_OnClient(AllCountriesDrawer.Settings countriesDrawerSettings)
        {
            allCountriesDrawer.Initialize(countriesDrawerSettings);
        }

        private void InitializeCountryController_OnClient()
        {
            countryController.Initialize(clientSettings.LocalPlayerId, phaseManager, game, players, Map,
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