using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Entities;
using UnityEngine;

namespace Terrix.Map
{
    public struct Hex
    {
        public HexType HexType { get; }
        public Vector2Int Position { get; }
        public Vector2Int[] NeighboursPositions { get; }
        [MaybeNull] public Player Owner { get; set; }
        
        public Hex(HexType hexType, Vector2Int position, Vector2Int mapSize)
        {
            HexType = hexType;
            Position = position;
            NeighboursPositions = MapUtilities.GetHexNeighborsPositions(position, mapSize);
            
            Owner = null;
        }

        public override string ToString()
        {
            return $"Hex_{HexType}_{Position.x}_{Position.y}";
        }
    }
}