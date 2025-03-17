using System;
using System.Collections;
using CustomUtilities.DataStructures;
using Terrix.DTO;
using Terrix.Map;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    public class GameController : MonoSingleton<GameController>
    {
        [Header("References")] 
        [SerializeField] private HexMapGenerator mapGenerator;
        [SerializeField] private TickGenerator tickGenerator;
        
        private IGameDataProvider gameDataProvider = new GameDataProvider();
        
        public HexMap Map { get; private set; }
        public GameReferee GameReferee { get; private set; }
        public PhaseManager PhaseManager { get; private set; }
        
        public AttackInvoker AttackInvoker { get; private set; }

        private void Start()
        {
            // временно
            var settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA)
            );
            StartGamePipeline(settings);
        }

        public void StartGamePipeline(Settings settings)
        {
            StartCoroutine(GamePipeline(settings));
        }
        
        private IEnumerator GamePipeline(Settings settings)
        {
            var gameData = gameDataProvider.Get();
            GameEvents.CreateInstance();
            AttackInvoker = new AttackInvoker();
            PhaseManager = new PhaseManager();
            GameReferee = new GameRefereeFactory(settings.GameModeSettings).Create();
            
            // Создание карты
            Map = mapGenerator.GenerateMap(settings.MapSettings);
            
            // Ивент о начале игры
            GameEvents.Instance.StartGame();
            
            // Выбор изначальной позиции
            
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        protected override void OnDestroyInternal()
        {
            GameEvents.ReleaseInstance();
        }


        public class Settings
        {
            public HexMapGenerator.Settings MapSettings { get; }
            public GameReferee.Settings GameModeSettings { get; }

            public Settings(
                HexMapGenerator.Settings mapSettings,
                GameReferee.Settings gameModeSettings)
            {
                MapSettings = mapSettings ?? throw new NullReferenceException($"{nameof(GameController)}.{nameof(Settings)}.{nameof(MapSettings)}");
                GameModeSettings = gameModeSettings ?? throw new NullReferenceException($"{nameof(GameController)}.{nameof(Settings)}.{nameof(GameModeSettings)}");;
            }
        }
    }
}