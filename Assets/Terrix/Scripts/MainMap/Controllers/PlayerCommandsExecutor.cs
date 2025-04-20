using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Extensions;
using System.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Network.DTO;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Controllers
{
    // только на сервере
    //На клиенте тоже, только пустышка нужна
    public class PlayerCommandsExecutor : NetworkBehaviour
    {
        [SerializeField] private MainMapEntryPoint mainMapEntryPoint;
        private HexMap map;
        private IPhaseManager phaseManager;
        private IPlayersProvider playerProvider;
        private IGameDataProvider gameDataProvider;
        private IAttackMassageEncoder attackMassageEncoder;
        private IAttackInvoker attackInvoker;

        private bool initialized;

        private TaskCompletionSource<bool> tcs;

        public void Initialize(HexMap map,
            IPhaseManager phaseManager,
            IPlayersProvider playersProvider,
            IGameDataProvider gameDataProvider,
            IAttackMassageEncoder attackMassageEncoder,
            IAttackInvoker attackInvoker)
        {
            this.map = map;
            this.phaseManager = phaseManager;
            this.playerProvider = playersProvider;
            this.gameDataProvider = gameDataProvider;
            this.attackMassageEncoder = attackMassageEncoder;
            this.attackInvoker = attackInvoker;

            initialized = true;
        }


        public async Task<bool> CanChooseInitialCountryPosition_OnClient(int playerId, Vector3Int pos)
        {
            tcs = new TaskCompletionSource<bool>();
            CanChooseInitialCountryPosition_ToServer(ClientManager.Connection, playerId, pos);
            var result = await tcs.Task;
            return result;
        }

        public bool CanChooseInitialCountryPosition_OnServer(int playerId, Vector3Int pos)
        {
            ValidateInitialization();

            var gameData = gameDataProvider.Get();

            var result = PlayerCheck() && PhaseCheck() && MapCheck();
            return result;

            bool PhaseCheck()
            {
                return phaseManager.CurrentPhase == GamePhaseType.Initial;
            }

            bool MapCheck()
            {
                return map.HasHex(pos) &&
                       OwnerCheck(map[pos]) &&
                       map[pos].GetHexData().CanCapture &&
                       map[pos].GetNeighbours().All(OwnerCheck);
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
                       map[pos].GetHexData().CanCapture &&
                       map[pos].GetNeighbours().All(OwnerCheck);
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
            ValidateInitialization();

            // сервер проводит дополнительную проверку
            if (!CanChooseInitialCountryPosition_OnServer(playerId, pos))
            {
                return;
            }

            var player = playerProvider.Find(playerId);
            ChooseInitialCountryPosition(player, pos, out _);
        }

        // [Server]
        private void ChooseInitialCountryPosition(Player player, Vector3Int pos, out Hex[] captureHexes)
        {
            var country = player.Country;
            var hexes = new List<Hex>(1 + 6) { map[pos] };
            foreach (var hex in map[pos].GetNeighbours())
            {
                if (hex.GetHexData().CanCapture)
                {
                    hexes.Add(hex);
                }
            }

            captureHexes = hexes.ToArray();
            country.ClearAndAdd(hexes.ToArray());
            mainMapEntryPoint.UpdateCountryHexes(new List<Country.UpdateSimplifiedCellsData>()
            {
                new(country.PlayerId,
                    hexes.Select(hex => new Country.SimplifiedCellChangeData(hex.Position, Country.UpdateCellMode.Add))
                        .ToList())
            });
            // new List<Country.SimplifiedCellChangeData>()))}hexes.Select(hex => new SimplifiedHex(hex.Position, player.ID))
            // .ToArray());
        }


        // [Server]
        public void ChooseRandomInitialCountryPosition(IEnumerable<Player> players,
            Action<Player, bool> onChoose = null)
        {
            ValidateInitialization();

            foreach (var player in players)
            {
                var randomHex = map.Hexes.Cast<Hex>().Where(hex => hex.GetHexData().CanCapture)
                    .Where(hex => hex.PlayerId == null && hex.GetNeighbours().All(neigh => neigh.PlayerId == null))
                    .RandomElementReservoirOrDefault();
                var success = randomHex is not null;

                if (success)
                {
                    ChooseInitialCountryPosition(player, randomHex.Position, out _);
                }

                onChoose?.Invoke(player, success);
            }
        }

        #region Attack

        //[Client]
        public void ExecuteAttack([NotNull] Attack attack)
        {
            var msg = attackMassageEncoder.Encode(attack);
            ExecuteAttack(msg);
        }

        //[Server]
        [ServerRpc(RequireOwnership = false)]
        private void ExecuteAttack(AttackMessage msg)
        {
            var attack = attackMassageEncoder.Decode(msg);
            attackInvoker.AddAttack(attack);
        }

        #endregion

        private void ValidateInitialization()
        {
            if (!initialized)
            {
                throw new InvalidOperationException($"{nameof(PlayerCommandsExecutor)} | не проинициализирован!");
            }
        }
    }
}