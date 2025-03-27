using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using Terrix.DTO;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Controllers
{
    // только на сервере
    //На клиенте тоже, только пустышка нужна
    public class PlayerCommandsExecutor : NetworkBehaviour
    {
        private HexMap map;
        private IPhaseManager phaseManager;
        private IPlayersProvider playerProvider;
        private IGameDataProvider gameDataProvider;

        private bool initialized;

        private TaskCompletionSource<bool> tcs;

        public void Initialize(HexMap map,
            IPhaseManager phaseManager,
            IPlayersProvider playersProvider,
            IGameDataProvider gameDataProvider)
        {
            this.map = map;
            this.phaseManager = phaseManager;
            this.playerProvider = playersProvider;
            this.gameDataProvider = gameDataProvider;

            initialized = true;
        }


        public async Task<bool> CanChooseInitialCountryPosition_OnClient(int playerId, Vector3Int pos)
        {
            tcs = new TaskCompletionSource<bool>();
            CanChooseInitialCountryPosition_ToServer(ClientManager.Connection, playerId, pos);
            var result = await tcs.Task;
            return result;
        }

        [TargetRpc]
        public void CanChooseInitialCountryPosition_ToTarget(NetworkConnection connection, bool result)
        {
            tcs.SetResult(result);
        }

        // на сервере и на клиенте
        // пока только на сервере
        [ServerRpc(RequireOwnership = false)]
        public void CanChooseInitialCountryPosition_ToServer(NetworkConnection connection, int playerId, Vector3Int pos)
        {
            ValidateInitialization();

            var gameData = gameDataProvider.Get();

            var result = PlayerCheck() && PhaseCheck() && MapCheck();
            CanChooseInitialCountryPosition_ToTarget(connection, result);


            bool PhaseCheck()
            {
                return phaseManager.CurrentPhase == GamePhaseType.Initial;
            }

            bool MapCheck()
            {
                return map.HasHex(pos) &&
                       OwnerCheck(map[pos]) &&
                       map[pos].GetHexData(gameData).CanCapture &&
                       map[pos].GetNeighbours(map).All(OwnerCheck);
            }

            bool OwnerCheck(Hex hex)
            {
                return hex.PlayerId == null || hex.PlayerId == playerId;
            }

            bool PlayerCheck()
            {
                var player = playerProvider.Find(playerId);
                return player is not null && player.Country is not null;
            }
        }

        // только на сервере
        [ServerRpc(RequireOwnership = false)]
        public void ChooseInitialCountryPosition(int playerId, Vector3Int pos)
        {
            var gameData = gameDataProvider.Get();

            var player = playerProvider.Find(playerId);
            var country = player.Country;

            var hexes = new List<Hex>(1 + 6) { map[pos] };
            foreach (var hex in map[pos].GetNeighbours(map))
            {
                if (hex.GetHexData(gameData).CanCapture)
                {
                    hexes.Add(hex);
                }
            }

            country.ClearAndAdd(hexes.ToArray());
        }

        private void ValidateInitialization()
        {
            if (!initialized)
            {
                throw new InvalidOperationException($"{nameof(PlayerCommandsExecutor)} | не проинициализирован!");
            }
        }
    }
}