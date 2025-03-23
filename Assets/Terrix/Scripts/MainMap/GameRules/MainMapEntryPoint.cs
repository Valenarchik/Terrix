using System;
using System.Collections;
using System.Linq;
using CustomUtilities.DataStructures;
using Terrix.Controllers;
using Terrix.Controllers.Country;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    // Нужно разбить на 2 класса, для сервера и для клиента
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
        private IGameReferee referee;
        private IAttackInvoker attackInvoker;
        private IPhaseManager phaseManager;
        private IPlayersProvider players;
        
        

        private void Start()
        {
            // временно
            var settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA),
                new PlayersAndBots(1, 0),
                new AllCountriesDrawer.Settings(new ZoneData[]
                {
                    new(0) {Color = Color.red}
                }),
                0
            );
            StartGamePipeline(settings);
        }

        public void StartGamePipeline(Settings settings)
        {
            StartCoroutine(GamePipeline(settings));
        }
        
        // Нужно так же делить на серверную и клиентскуюя часть
        private IEnumerator GamePipeline(Settings settings)
        {
            Initialize(settings);
            
            // Ивент о начале игры
            events.StartGame();
            
            // Выбор изначальной позиции
            phaseManager.NextPhase();
            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        private void Initialize(Settings settings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);

            events = new GameEvents();
            attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();
            players = new PlayersProvider(playersFactory.CreatePlayers(settings.PlayersCount));
            referee = gameRefereeFactory.Create(players.GetAll());
            
            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());
            allCountriesDrawer.Initialize(settings.CountryDrawerSettings);
            countryController.Initialize(settings.LocalPlayerId, phaseManager, events);
            cameraController.Initialize(events);
            
            map = mapGenerator.GenerateMap(settings.MapSettings);
            
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20
            };

            GUI.Label(new Rect(0,0,300,25), $"CurrentPhase is {phaseManager.CurrentPhase}", labelStyle);
        }
#endif


        public class Settings
        {
            public HexMapGenerator.Settings MapSettings { get; } // сервер
            public GameReferee.Settings GameModeSettings { get; } // сервер
            public PlayersAndBots PlayersCount { get; } // сервер
            public AllCountriesDrawer.Settings CountryDrawerSettings { get; } // клиент
            public int LocalPlayerId { get; } // клиент

            public Settings(
                HexMapGenerator.Settings mapSettings,
                GameReferee.Settings gameModeSettings,
                PlayersAndBots playersCount,
                AllCountriesDrawer.Settings countryDrawerSettings,
                int localPlayerId)
            {
                MapSettings = mapSettings ?? throw new NullReferenceException($"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(MapSettings)}");
                GameModeSettings = gameModeSettings ?? throw new NullReferenceException($"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(GameModeSettings)}");
                PlayersCount = playersCount;
                CountryDrawerSettings = countryDrawerSettings?? throw new NullReferenceException($"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(CountryDrawerSettings)}");
                LocalPlayerId = localPlayerId;
            }
        }
    }
}