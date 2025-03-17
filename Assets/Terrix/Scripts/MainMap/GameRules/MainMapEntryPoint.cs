using System;
using System.Collections;
using CustomUtilities.DataStructures;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    public class MainMapEntryPoint : MonoSingleton<MainMapEntryPoint>
    {
        [Header("References")] 
        [SerializeField] private HexMapGenerator mapGenerator;
        [SerializeField] private TickGenerator tickGenerator;

        private IGameDataProvider gameDataProvider;
        private IPlayersFactory playersFactory;
        private IGameRefereeFactory gameRefereeFactory;

        private void Start()
        {
            // временно
            var settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA),
                new PlayersAndBots(1, 0)
            );
            StartGamePipeline(settings);
        }

        public void StartGamePipeline(Settings settings)
        {
            StartCoroutine(GamePipeline(settings));
        }
        
        private IEnumerator GamePipeline(Settings settings)
        {
            ResolveDependencies(settings);
            
            // Создание карты
            MainMap.Map = mapGenerator.GenerateMap(settings.MapSettings);
            
            // Ивент о начале игры
            MainMap.Events.StartGame();
            
            // Выбор изначальной позиции
            MainMap.PhaseManager.NextPhase();
            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        protected override void OnDestroyInternal()
        {
            
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