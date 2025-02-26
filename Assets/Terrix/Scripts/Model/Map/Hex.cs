using System;
using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Model.Entities;
using UnityEngine;

namespace Terrix.Model
{
    public class Hex
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

        public Hex([NotNull] HexData data, Vector2Int position, Vector2Int[] neighboursPositions)
        {
            Data = data;
            Position = position;
            NeighboursPositions = neighboursPositions;
        }

        public override string ToString()
        {
            return $"Cell_{Position.x}_{Position.y}";
        }
    }
}