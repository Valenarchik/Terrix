using System;
using System.Collections;
using System.Linq;
using CustomUtilities.DataStructures;
using Terrix.Controllers;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Terrix.Game.GameRules
{
    public class MainMapEntryPoint : MonoSingleton<MainMapEntryPoint>
    {
        [Header("References")]
        [SerializeField] private HexMapGenerator mapGenerator;

        [SerializeField] private TickGenerator tickGenerator;
        [SerializeField] private AllCountriesHandler allCountriesHandler;
        [SerializeField] private AllCountriesDrawer allCountriesDrawer;
        [SerializeField] private CountryController countryController;
        [SerializeField] private MainMapCameraController cameraController;
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

        private void Start()
        {
            // временно
            var serverSettings = new ServerSettings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA),
                new PlayersAndBots(1, 100),
                new AllCountriesDrawer.Settings(Enumerable.Range(0, 101)
                    .Select(i => new ZoneData(i)
                    {
                        Color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))
                    })
                    .ToArray())
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
            var gameData = gameDataProvider.Get();
            events.StartGame();

            phaseManager.NextPhase();
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

        private void Initialize(ServerSettings serverSettings, ClientSettings clientSettings)
        {
            gameDataProvider = new GameDataProvider();
            events = new GameEvents();
            attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();

            playersFactory = new PlayersFactory(gameDataProvider);
            players = new PlayersProvider(playersFactory.CreatePlayers(serverSettings.PlayersCount));
            gameRefereeFactory = new GameRefereeFactory(serverSettings.GameModeSettings, players);
            referee = gameRefereeFactory.Create();
            countriesCollector = new CountriesCollector(players);

            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());
            allCountriesDrawer.Initialize(serverSettings.CountryDrawerSettings);
            countryController.Initialize(clientSettings.LocalPlayerId, phaseManager, events);
            cameraController.Initialize(events);

            map = mapGenerator.GenerateMap(serverSettings.MapSettings);

            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
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