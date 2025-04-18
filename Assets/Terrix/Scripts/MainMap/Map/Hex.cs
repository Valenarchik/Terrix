using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.DTO;
using Terrix.Game.GameRules;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Map
{
    public class Hex : IEquatable<Hex>
    {
        public HexMap HexMap { get; set; }
        private readonly IGameDataProvider gameData;
        private readonly IPlayersProvider players;

        public Hex[] Neighbours { get; private set; }

        public HexType HexType { get; }
        public Vector3Int Position { get; }
        public Vector3 WorldPosition { get; }
        public int? PlayerId { get; set; }
        public IPlayersProvider Players => players;

        public Hex(
            HexType hexType,
            Vector3Int position,
            Vector3 worldPosition,
            HexMap hexMap,
            IGameDataProvider gameData,
            IPlayersProvider players)
        {
            HexType = hexType;
            Position = position;
            WorldPosition = worldPosition;
            PlayerId = null;
            this.HexMap = hexMap;
            this.gameData = gameData;
            this.players = players;
        }

        public IEnumerable<Hex> GetNeighbours()
        {
            if (Neighbours == null)
            {
                Neighbours = MapUtilities.GetHexNeighborsPositions(Position)
                    .Where(pos => HexMap.HasHex(pos))
                    .Select(pos => HexMap[pos])
                    .ToArray();
            }

            return Neighbours;
        }

        public GameHexData GetHexData()
        {
            return gameData.Get().CellsStats[HexType];
        }

        //TODO возможно нужна переработка
        public Hex(HexType hexType, Vector3Int position, Vector3 worldPosition,
            int? playerId)
        {
            HexType = hexType;
            Position = position;
            WorldPosition = worldPosition;
            PlayerId = playerId;
            this.gameData = new GameDataProvider();
            // this.Neighbours = neighbours;
        }
        public Hex(HexType hexType, Vector3Int position, Vector3 worldPosition,
            int? playerId,
            IPlayersProvider players)
        {
            HexType = hexType;
            Position = position;
            WorldPosition = worldPosition;
            PlayerId = playerId;
            this.gameData = new GameDataProvider();
            this.players = players;
            // this.Neighbours = neighbours;
        }

        public float GetCost()
        {
            return GetCost(PlayerId);
        }

        public float GetCost(int? playerId)
        {
            if (playerId == null)
            {
                return gameData.Get().BaseCostOfNeutralLends * GetHexData().Resist;
            }

            return players.Find(playerId.Value).Country.DensePopulation * GetHexData().Resist;
        }

        public override string ToString()
        {
            return $"Hex_{HexType}_{Position.x}_{Position.y}_{Position.z}";
        }

        public bool Equals(Hex other)
        {
            return other != null && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is Hex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}