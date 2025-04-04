using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.DTO;
using Terrix.Game.GameRules;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Map
{
    public class Hex: IEquatable<Hex>
    {
        private readonly HexMap hexMap;
        private readonly IGameDataProvider gameData;
        private readonly IPlayersProvider players;

        private Hex[] neighbours;
        
        public HexType HexType { get; }
        public Vector3Int Position { get; }
        public Vector3 WorldPosition { get; }
        public int? PlayerId { get; set; }

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
            this.hexMap = hexMap;
            this.gameData = gameData;
            this.players = players;
        }

        public IEnumerable<Hex> GetNeighbours()
        {
            if (neighbours == null)
            {
                neighbours = MapUtilities.GetHexNeighborsPositions(Position)
                    .Where(pos => hexMap.HasHex(pos))
                    .Select(pos => hexMap[pos])
                    .ToArray();
            }
            
            return neighbours;
        }

        public GameHexData GetHexData()
        {
            return gameData.Get().CellsStats[HexType];
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