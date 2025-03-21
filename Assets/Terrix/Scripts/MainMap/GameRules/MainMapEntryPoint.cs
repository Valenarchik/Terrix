using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
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
        // [SerializeField] private HexMapGenerator hexMapGenerator;
        [SerializeField] private MainMapCameraController cameraController;

        // private IGameDataProvider gameDataProvider = new GameDataProvider();

        // public HexMap Map { get; private set; }
        // public GameReferee GameReferee { get; private set; }
        // public PhaseManager PhaseManager { get; private set; }
        //
        // public AttackInvoker AttackInvoker { get; private set; }

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
            // Debug.Log("Server started");
            var settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA),
                new PlayersAndBots(1, 0)
            );

            // Generate_OnServer(settings); //
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

        public override void OnStartClient()
        {
            base.OnStartClient();
            // GameEvents.CreateInstance(); //
            ResolveDependencies_OnClient();
            UpdateClients_ToServer(ClientManager.Connection);
            MainMap.Events.StartGame();
            // StartGamePipeline();
            // Debug.Log("MainMapEntryPoint Client");
        }

        [ServerRpc(RequireOwnership = false)]
        void UpdateClients_ToServer(NetworkConnection connection)
        {
            // ResolveDependencies_OnClient(MainMap.Map);
            // UpdateMap_ToObserver(MainMap.Map);
            UpdateMap_ToTarget(connection, MainMap.Map);
        }

        // [ObserversRpc]
        // void UpdateMap_ToObserver(HexMap map)
        // {
        //     MainMap.Map = map;
        //     mapGenerator.UpdateMap(map);
        // }

        [TargetRpc]
        void UpdateMap_ToTarget(NetworkConnection connection, HexMap map)
        {
            MainMap.Map = map;
            mapGenerator.UpdateMap(map);
            Debug.Log("Main map Target");
        }

        [ObserversRpc]
        public void StartGamePipeline()
        {
            StartCoroutine(GamePipeline());
        }

        private IEnumerator GamePipeline()
        {
            MainMap.PhaseManager.NextPhase();
            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        private void ResolveDependencies_OnServer(Settings settings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);

            MainMap.Map = mapGenerator.GenerateMap(settings.MapSettings);
            MainMap.Referee = gameRefereeFactory.Create(playersFactory.CreatePlayers(settings.PlayersCount));
        }

        private void ResolveDependencies_OnClient()
        {
            gameDataProvider = new GameDataProvider();

            MainMap.Events = new GameEvents();
            MainMap.AttackInvoker = new AttackInvoker();
            MainMap.PhaseManager = new PhaseManager();
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
                MapSettings = mapSettings ??
                              throw new NullReferenceException(
                                  $"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(MapSettings)}");
                GameModeSettings = gameModeSettings ??
                                   throw new NullReferenceException(
                                       $"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(GameModeSettings)}");
                PlayersCount = playersCount;
            }
        }
    }
}