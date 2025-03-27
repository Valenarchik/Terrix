using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Terrix.Controllers;
using System.Linq;
using FishNet.Transporting;
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
        private Dictionary<NetworkConnection, int> playersIds = new();
        private Settings settings;

        public override void OnStartServer()
        {
            // Надо подтягивать эти данные откуда-то. наверно из лобби
            base.OnStartServer();
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManagerOnRemoteConnectionState_OnServer;
            var gameMode = GameModeType.FFA;
            var playersAndBots = new PlayersAndBots(lobby.PlayersMaxCount, 0);
            settings = new Settings(
                mapGenerator.DefaultSettingsSo.Get(),
                new GameReferee.Settings(gameMode),
                playersAndBots,
                // new AllCountriesDrawer.Settings(new ZoneData[]
                // {
                //     new(0) { Color = new Color(1, 0, 0, 1f) },
                //     new(1) { Color = new Color(0, 1, 0, 1f) },
                //     // new(2) { Color = new Color(0, 0, 1, 1f) }
                // }),
                new AllCountriesDrawer.Settings(
                    Enumerable.Range(0, lobby.PlayersMaxCount).Select(i => new ZoneData(i)).ToArray()),
                0
            );
            Initialize_OnServer(settings);
            lobby.LobbyStateMachine.OnStateChanged += LobbyStateMachineOnStateChanged;
        }

        protected void ServerManagerOnRemoteConnectionState_OnServer(NetworkConnection conn,
            RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Stopped:
                    playersIds.Remove(conn);
                    break;
                case RemoteConnectionState.Started:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LobbyStateMachineOnStateChanged(LobbyState state)
        {
            if (state == lobby.LobbyStateMachine.LobbyStartingState)
            {
                StartGamePipeline();
            }
        }

        // Нужно так же делить на серверную и клиентскуюя часть
        public override void OnStartClient()
        {
            base.OnStartClient();
            var color = PlayerDataHolder.Color;
            Initialize_OnClient_ToServer(ClientManager.Connection, color);
        }

        public void StartGamePipeline()
        {
            StartCoroutine(GamePipeline());
        }

        private IEnumerator GamePipeline()
        {
            // Initialize(settings);

            // Ивент о начале игры
            // events.StartGame();

            // Выбор изначальной позиции
            InitializeAllCountriesDrawer_ToObserver(settings.CountryDrawerSettings);

            phaseManager.NextPhase();
            ChangePhase();
            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TimeForChooseFirstCountryPosition.Seconds);
        }

        [ObserversRpc]
        private void ChangePhase()
        {
            phaseManager.NextPhase();
        }

        private void Initialize_OnServer(Settings settings)
        {
            gameDataProvider = new GameDataProvider();
            playersFactory = new PlayersFactory(gameDataProvider);
            gameRefereeFactory = new GameRefereeFactory(settings.GameModeSettings);
            map = mapGenerator.GenerateMap(settings.MapSettings);
            phaseManager = new PhaseManager();
            referee = gameRefereeFactory.Create(playersFactory.CreatePlayers(settings.PlayersCount));
            players = new PlayersProvider(playersFactory.CreatePlayers(settings.PlayersCount));
            //TODO хз куда это
            allCountriesHandler.Initialize(players.GetAll().Select(p => p.Country).ToArray());
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
        }

        [ServerRpc(RequireOwnership = false)]
        private void Initialize_OnClient_ToServer(NetworkConnection connection, Color color)
        {
            int id = -1;
            for (int i = 0; i < players.GetAll().Length; i++)
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

            settings.CountryDrawerSettings.Zones[id].Color = color;

            Initialize_OnClient_ToTarget(connection, map, id);
        }


        [TargetRpc]
        private void Initialize_OnClient_ToTarget(NetworkConnection connection, HexMap map, int id)
        {
            settings = new Settings(id);
            this.map = map;
            mapGenerator.UpdateMap(map);
            gameDataProvider = new GameDataProvider();

            events = new GameEvents();
            attackInvoker = new AttackInvoker();
            phaseManager = new PhaseManager();
            countryController.Initialize(settings.LocalPlayerId, phaseManager, events);
            cameraController.Initialize(events);
            commandsExecutor.Initialize(map, phaseManager, players, gameDataProvider);
            events.StartGame();
        }

        [ObserversRpc]
        private void InitializeAllCountriesDrawer_ToObserver(AllCountriesDrawer.Settings countriesDrawerSettings)
        {
            allCountriesDrawer.Initialize(countriesDrawerSettings);
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20
            };

            GUI.Label(new Rect(0, 0, 300, 25), $"CurrentPhase is {phaseManager.CurrentPhase}", labelStyle);
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
                CountryDrawerSettings = countryDrawerSettings ??
                                        throw new NullReferenceException(
                                            $"{nameof(MainMapEntryPoint)}.{nameof(Settings)}.{nameof(CountryDrawerSettings)}");
                LocalPlayerId = localPlayerId;
            }

            // для клиента
            public Settings(
                int localPlayerId)
            {
                LocalPlayerId = localPlayerId;
            }
        }
    }
}