using System;
using System.Collections;
using System.Linq;
using CustomUtilities.DataStructures;
using CustomUtilities.Extensions;
using Terrix.Controllers;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Network.DTO;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Terrix.Game.GameRules
{
    public class MainMapEntryPoint : MonoSingleton<MainMapEntryPoint>
    {
        [Header("References")]
        [SerializeField] private TickGenerator tickGenerator;
        [SerializeField] private AllCountriesHandler allCountriesHandler;
        [SerializeField] private AllCountriesDrawer allCountriesDrawer;
        [SerializeField] private CountryController countryController;
        [SerializeField] private MainMapCameraController cameraController;
        [SerializeField] private PlayerCommandsExecutor commandsExecutor;

        [Header("MapReferences")]
        [SerializeField] private HexMapGenerator mapGenerator;
        [SerializeField] private HexMapRenderer mapRenderer;
        [SerializeField] private HexMapGeneratorSettingsSO defaultMapGenerationSettings;
        
        private IGameDataProvider gameDataProvider;
        private IPlayersFactory playersFactory;
        private IGameRefereeFactory gameRefereeFactory;

        private IGame game;
        private HexMap map;
        private ICountriesCollector countriesCollector;
        private IGameReferee referee;
        private IAttackInvoker attackInvoker;
        private IPhaseManager phaseManager;
        private IPlayersProvider players;
        private IAttackMassageEncoder attackMassageEncoder;

        private void Start()
        {
            var zones = Enumerable.Range(0, 101)
                .Select(i =>
                {
                    var randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    
                    return new ZoneData(i)
                    {
                        Color = randomColor.WithAlpha(0.5f),
                        BorderColor = randomColor
                    };
                }).ToArray();
            
            // временно
            var serverSettings = new ServerSettings(
                defaultMapGenerationSettings.Get(),
                new GameReferee.Settings(GameModeType.FFA, 0.05f),
                new PlayersAndBots(1, 100),
                new AllCountriesDrawer.Settings
                {
                    Zones = zones,
                    DragZone = new ZoneData(AllCountriesDrawer.DRAG_ZONE_ID)
                    {
                        Color = zones[0].Color.Value
                    }   
                }
            );

            var clientSettings = new ClientSettings(0);

            StartGamePipeline(serverSettings, clientSettings);
        }

        public void StartGamePipeline(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            StartCoroutine(GamePipeline(serverSettings, clientSettings));
        }

        private IEnumerator GamePipeline(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            Initialize(serverSettings, clientSettings);
            
            mapRenderer.RenderMap(map);
            
            var gameData = gameDataProvider.Get();
            
            game.StartGame();

            phaseManager.NextPhase();
            BotsChooseRandomPositions();
            yield return new WaitForSeconds((float) gameData.TimeForChooseFirstCountryPosition.TotalSeconds);
            NotInitializedPlayersChooseRandomPositions();

            phaseManager.NextPhase();
            tickGenerator.InitializeLoop(new TickGenerator.TickHandlerTuple[]
            {
                new(countriesCollector, gameData.TickHandlers[TickHandlerType.CountriesCollectorHandler]),
                new(attackInvoker, gameData.TickHandlers[TickHandlerType.AttackHandler]),
                new(referee, gameData.TickHandlers[TickHandlerType.RefereeHandler])
            });

            // Ждем пока игра не закончится
            yield return new WaitWhile(() => !game.GameOver);
            phaseManager.NextPhase();
            tickGenerator.StopLoop();
        }

        private void Initialize(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            gameDataProvider = new GameDataProvider();
            game = new Game();
            phaseManager = new PhaseManager();
            

            allCountriesDrawer.Initialize(serverSettings.CountryDrawerSettings);
            cameraController.Initialize(game);

            playersFactory = new PlayersFactory(gameDataProvider);
            players = new PlayersProvider(playersFactory.CreatePlayers(serverSettings.PlayersCount));
            mapGenerator.Initialize(gameDataProvider, players);
            map = mapGenerator.GenerateMap(serverSettings.MapSettings);
            countryController.Initialize(clientSettings.LocalPlayerId, phaseManager, game, players, map, gameDataProvider);

            attackInvoker = new AttackInvoker();
            
            gameRefereeFactory = new GameRefereeFactory(serverSettings.GameModeSettings, players, game);
            referee = gameRefereeFactory.Create();
            countriesCollector = new CountriesCollector(players);

            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());

            attackMassageEncoder = new AttackMassageEncoder(players, map);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider, attackMassageEncoder, attackInvoker);
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