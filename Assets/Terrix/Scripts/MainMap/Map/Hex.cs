using System;
using System.Collections.Generic;
using Terrix.DTO;
using Terrix.Game.GameRules;
using UnityEngine;

namespace Terrix.Map
{
    public class Hex: IEquatable<Hex>
    {
        public HexType HexType { get; }
        public Vector3Int Position { get; }
        public Vector3Int[] NeighboursPositions { get; }
        public int? PlayerId { get; set; }
        
        public Hex(HexType hexType, Vector3Int position)
        {
            HexType = hexType;
            Position = position;
            NeighboursPositions = MapUtilities.GetHexNeighborsPositions(position);
            PlayerId = null;
        }

        public IEnumerable<Hex> GetNeighbours(HexMap hexMap)
        {
            foreach (var pos in NeighboursPositions)
            {
                if (hexMap.HasHex(pos))
                {
                    yield return hexMap[pos];
                }
            }
        }

        public HexData GetHexData(GameData gameData)
        {
            return gameData.CellsStats[HexType];
        }

        public float GetCost(IPlayersProvider playersProvider, GameData gameData)
        {
            if (PlayerId == null)
            {
                return gameData.BaseCostOfNeutralLends * GetHexData(gameData).Resist;
            }
            else
            {
                return playersProvider.Find(PlayerId.Value).Country.DensePopulation * GetHexData(gameData).Resist;
            }
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