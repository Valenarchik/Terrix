using System;
using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using Terrix.Controllers;
using System.Linq;
using Terrix.Controllers.Country;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Networking;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    // Нужно разбить на 2 класса, для сервера и для клиента
    // public class MainMapEntryPoint : MonoSingleton<MainMapEntryPoint>
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
        
        

        
        public override void OnStartServer()
        {
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

            ResolveDependencies_OnServer(settings);
            lobby.LobbyStateMachine.OnStateChanged += LobbyStateMachineOnOnStateChanged;
        }

        private void LobbyStateMachineOnOnStateChanged(LobbyState state)
        {
            if (state == lobby.LobbyStateMachine.LobbyStartingState)
            {
                StartGamePipeline();
            }
        }
        // Нужно так же делить на серверную и клиентскуюя часть
        public override void OnStartClient()
        {
            ResolveDependencies_OnClient();
            UpdateClients_ToServer(ClientManager.Connection);
            mainMap.Events.StartGame();
        }

        [ServerRpc(RequireOwnership = false)]
        void UpdateClients_ToServer(NetworkConnection connection)
        {
            UpdateMap_ToTarget(connection, mainMap.Map);
        }

        [TargetRpc]
        void UpdateMap_ToTarget(NetworkConnection connection, HexMap map)
        {
            mainMap.Map = map;
            mapGenerator.UpdateMap(map);
            Debug.Log("Main map Target");
        }

        [ObserversRpc]
        public void StartGamePipeline()
        {
            StartCoroutine(GamePipeline());
        }
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
        //Назвать Initialize
        private void ResolveDependencies_OnServer(Settings settings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);
            map = mapGenerator.GenerateMap(settings.MapSettings);
            referee = gameRefereeFactory.Create(playersFactory.CreatePlayers(settings.PlayersCount));
            players = new PlayersProvider(playersFactory.CreatePlayers(settings.PlayersCount));
            //TODO хз куда это
            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());
            allCountriesDrawer.Initialize(settings.CountryDrawerSettings);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
        }

        private void ResolveDependencies_OnClient()
        {
            gameDataProvider = new GameDataProvider();

            events = new GameEvents();
            attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();
            //TODO не уверен, что это сюда
            countryController.Initialize(settings.LocalPlayerId, phaseManager, events);
            cameraController.Initialize(events);
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
                MapSettings = mapSettings ??
                              throw new NullReferenceException(
                                  $"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(MapSettings)}");
                GameModeSettings = gameModeSettings ??
                                   throw new NullReferenceException(
                                       $"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(GameModeSettings)}");
                PlayersCount = playersCount;
                CountryDrawerSettings = countryDrawerSettings?? throw new NullReferenceException($"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(CountryDrawerSettings)}");
                LocalPlayerId = localPlayerId;
            }
        }
    }
}