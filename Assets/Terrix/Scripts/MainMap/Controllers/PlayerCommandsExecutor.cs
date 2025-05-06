using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Extensions;
using System.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using JetBrains.Annotations;
using Priority_Queue;
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

        private TaskCompletionSource<bool> canChoosePositionTcs;

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
            canChoosePositionTcs = new TaskCompletionSource<bool>();
            CanChooseInitialCountryPosition_ToServer(ClientManager.Connection, playerId, pos);
            var result = await canChoosePositionTcs.Task;
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
        private void CanChooseInitialCountryPosition_ToTarget(NetworkConnection connection, bool result)
        {
            canChoosePositionTcs.SetResult(result);
        }

        // на сервере и на клиенте
        // пока только на сервере
        [ServerRpc(RequireOwnership = false)]
        public void CanChooseInitialCountryPosition_ToServer(NetworkConnection connection, int playerId, Vector3Int pos)
        {
            ValidateInitialization();

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
        public void ChooseInitialCountryPosition_ToServer(int playerId, Vector3Int pos)
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

        public Hex[] StretchBorders(
            Hex startHex,
            Hex endHex,
            Country country,
            out int? attackTarget,
            out float attackPoints)
        {
            var direction = (endHex.WorldPosition - startHex.WorldPosition).normalized;
            var result = new List<Hex>();
            var visited = new HashSet<Hex>();
            var priorityQueue = new SimplePriorityQueue<Hex, float>();

            var seed = startHex.GetNeighbours()
                .Where(neighbour => !country.Contains(neighbour) || !neighbour.GetHexData().CanCapture)
                .Select(hex => new { Hex = hex, Direction = (hex.WorldPosition - startHex.WorldPosition).normalized })
                .OrderByDescending(hex => Vector3.Dot(hex.Direction, direction))
                .Select(hex => hex.Hex)
                .First();

            attackTarget = seed.PlayerId;

            priorityQueue.Enqueue(seed, 0);

            var remainingPoints = country.Population;
            var targetReached = false;

            while (priorityQueue.Count > 0 && remainingPoints > 0 && !targetReached)
            {
                priorityQueue.TryDequeue(out var cell);
                if (visited.Contains(cell) || country.Contains(cell))
                {
                    continue;
                }

                var cellCost = cell.GetCost();
                if (cellCost > remainingPoints)
                {
                    continue;
                }

                result.Add(cell);

                if (endHex.Equals(cell))
                {
                    targetReached = true;
                }

                remainingPoints -= cellCost;
                visited.Add(cell);


                foreach (var neighbour in cell.GetNeighbours())
                {
                    if (country.Contains(neighbour)
                        || visited.Contains(neighbour)
                        || !neighbour.GetHexData().CanCapture
                        || attackTarget != neighbour.PlayerId)
                    {
                        continue;
                    }

                    var priority = CalculatePriority(neighbour);
                    priorityQueue.Enqueue(neighbour, -priority);
                }
            }

            attackPoints = country.Population - remainingPoints;
            return result.ToArray();

            float CalculatePriority(Hex current)
            {
                var alpha = 1.2f;
                var beta = 10f;
                var delta = current.WorldPosition - startHex.WorldPosition;

                var projection = Vector3.Dot(delta.normalized, direction);

                var distance = delta.magnitude;
                var distanceFactor = 1f / (1f + distance);

                return projection * alpha + distanceFactor * beta;
            }
        }
    }
}