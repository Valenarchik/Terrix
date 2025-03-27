using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Extensions;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Controllers
{
    // только на сервере
    public class PlayerCommandsExecutor : MonoBehaviour
    {
        private HexMap map;
        private IPhaseManager phaseManager;
        private IPlayersProvider playerProvider;
        private IGameDataProvider gameDataProvider;

        private bool initialized;

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

        // на сервере и на клиенте
        public bool CanChooseInitialCountryPosition(int playerId, Vector3Int pos)
        {
            ValidateInitialization();
            
            var gameData = gameDataProvider.Get();

            return PlayerCheck() && PhaseCheck() && MapCheck();

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

        // [Server]
        public void ChooseInitialCountryPosition(int playerId, Vector3Int pos)
        {
            ValidateInitialization();
            
            // сервер проводит дополнительную проверку
            if (!CanChooseInitialCountryPosition(playerId, pos))
            {
                return;
            }
            
            var player = playerProvider.Find(playerId);
            ChooseInitialCountryPosition(player, pos);
        }

        // [Server]
        private void ChooseInitialCountryPosition(Player player, Vector3Int pos)
        {
            var gameData = gameDataProvider.Get();
            var country = player.Country;
            var hexes = new List<Hex>(1 + 6) {map[pos]};
            foreach (var hex in map[pos].GetNeighbours(map))
            {
                if (hex.GetHexData(gameData).CanCapture)
                {
                    hexes.Add(hex);
                }
            }
            
            country.ClearAndAdd(hexes.ToArray());
        }

        // [Server]
        public void ChooseRandomInitialCountryPosition(IEnumerable<Player> players, Action<Player, bool> onChoose = null)
        {
            ValidateInitialization();

            foreach (var player in players)
            {
                var randomHex = map.CanCaptureHexes
                    .Where(hex => hex.PlayerId == null && hex.GetNeighbours(map).All(neigh => neigh.PlayerId == null))
                    .RandomElementOrDefault();
                var success = randomHex is not null;
                
                if (success)
                {
                    ChooseInitialCountryPosition(player, randomHex.Position);
                }
                
                onChoose?.Invoke(player, success);
            }
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