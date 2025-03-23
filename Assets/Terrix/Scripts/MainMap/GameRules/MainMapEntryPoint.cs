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
        [SerializeField] private MainMap mainMap;
        [SerializeField] private Lobby lobby;
        [SerializeField] private MainMapCameraController cameraController;

        private IGameDataProvider gameDataProvider;
        private IPlayersFactory playersFactory;
        private IGameRefereeFactory gameRefereeFactory;


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
            var settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(GameModeType.FFA),
                new PlayersAndBots(1, 0)
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

        private IEnumerator GamePipeline()
        {
            mainMap.PhaseManager.NextPhase();
            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        private void ResolveDependencies_OnServer(Settings settings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);
            mainMap.Map = mapGenerator.GenerateMap(settings.MapSettings);
            mainMap.Referee = gameRefereeFactory.Create(playersFactory.CreatePlayers(settings.PlayersCount));
        }

        private void ResolveDependencies_OnClient()
        {
            gameDataProvider = new GameDataProvider();

            mainMap.Events = new GameEvents();
            mainMap.AttackInvoker = new AttackInvoker();
            mainMap.PhaseManager = new PhaseManager();
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