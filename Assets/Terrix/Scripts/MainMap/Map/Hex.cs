using System;
using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Entities;
using UnityEngine;

namespace Terrix.Map
{
    public struct Hex
    {
        [NotNull] public HexData Data { get; private set; }
        public HexType HexType => Data.HexType;
        public float Income => Data.Income;
        public float Resist => Data.Resist;
        public bool CanCapture => Data.CanCapture;
        public bool IsSeeTile => Data.IsSeeTile;

        public Vector2Int Position { get; private set; }
        public Vector2Int[] NeighboursPositions { get; }
        [MaybeNull] public Player Owner { get; private set; }

        public Action<Hex> OwnerChanged;

        public Hex([NotNull] HexData data, Vector2Int position, Vector2Int mapSize)
        {
            Data = data;
            Position = position;
            NeighboursPositions = MapUtilities.GetHexNeighborsPositions(position, mapSize);
            
            OwnerChanged = null;
            Owner = null;
        }
        public Hex([NotNull] HexData data, Vector2Int position, Vector2Int[] neighboursPositions, Player owner)
        {
            Data = data;
            Position = position;
            NeighboursPositions = neighboursPositions;
            
            OwnerChanged = null;
            Owner = owner;
        }

        public override string ToString()
        {
            return $"Hex_{Position.x}_{Position.y}";
        }
    }
}