using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Terrix.Controllers;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Networking;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    public class MainMapEntryPoint : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private HexMapGenerator mapGenerator;
        [SerializeField] private TickGenerator tickGenerator;

        private IGameDataProvider gameDataProvider;
        private IPlayersFactory playersFactory;
        private IGameRefereeFactory gameRefereeFactory;
        [SerializeField] private Lobby lobby;
        [SerializeField] private HexMapGenerator hexMapGenerator;
        [SerializeField] private MainMapCameraController cameraController;

        private IGameDataProvider gameDataProvider = new GameDataProvider();

        public HexMap Map { get; private set; }
        public GameReferee GameReferee { get; private set; }
        public PhaseManager PhaseManager { get; private set; }

        public AttackInvoker AttackInvoker { get; private set; }

        private void Start()
        {
            // временно
            // var settings = new Settings(
            //     mapGenerator.DefaultSettingsSo.Get(),
            //     new GameReferee.Settings(GameModeType.FFA)
            // );
            // StartGamePipeline(settings);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Server started");
            var settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA),
                new PlayersAndBots(1, 0)
            );
            Generate_OnServer(settings); //
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            GameEvents.CreateInstance(); //
            UpdateMap_ToServer();
        }

        [ServerRpc(RequireOwnership = false)]
        void UpdateMap_ToServer()
        {
            UpdateMap_ToObserver(Map);
        }

        [ObserversRpc]
        void UpdateMap_ToObserver(HexMap map)
        {
            Map = map;
            hexMapGenerator.UpdateMap(map);
        }

        public void StartGamePipeline(Settings settings)
        {
            StartCoroutine(GamePipeline(settings));
        }

        private IEnumerator GamePipeline(Settings settings)
        {
            // Ивент о начале игры
            MainMap.Events.StartGame();
            
            // Выбор изначальной позиции
            MainMap.PhaseManager.NextPhase();
            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        private void Generate_OnServer(Settings settings)
        {
            // TODO убрать
            // GameEvents.CreateInstance(); //
            AttackInvoker = new AttackInvoker();
            PhaseManager = new PhaseManager();
            GameReferee = new GameRefereeFactory(settings.GameModeSettings).Create(new List<Player>());

            // Создание карты
            Map = mapGenerator.GenerateMap(settings.MapSettings);
            ResolveDependencies(settings)
        }

        private void ResolveDependencies(Settings settings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);

            MainMap.Events = new GameEvents();
            MainMap.AttackInvoker = new AttackInvoker();
            MainMap.PhaseManager = new PhaseManager();
            MainMap.Referee = gameRefereeFactory.Create(playersFactory.CreatePlayers(settings.PlayersCount));
        }


        public class Settings
        {
            public HexMapGenerator.Settings MapSettings { get; }
            public GameReferee.Settings GameModeSettings { get; }
            public PlayersAndBots PlayersCount { get; }

            public Settings(
                HexMapGenerator.Settings mapSettings,
                GameReferee.Settings gameModeSettings,
                PlayersAndBots playersCount)
            {
                MapSettings = mapSettings ?? throw new NullReferenceException($"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(MapSettings)}");
                GameModeSettings = gameModeSettings ?? throw new NullReferenceException($"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(GameModeSettings)}");
                PlayersCount = playersCount;
            }
        }
    }
}