using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;
using UnityEngine;

namespace Terrix.Map
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

        public Hex([NotNull] HexData data, Vector2Int position, Vector2Int mapSize)
        {
            Data = data;
            Position = position;

            NeighboursPositions = Position.y % 2 == 0
                ? new[]
                {
                    Position + new Vector2Int(-1, 0),
                    Position + new Vector2Int(-1, 1),
                    Position + new Vector2Int(0, 1),
                    Position + new Vector2Int(1, 0),
                    Position + new Vector2Int(0, -1),
                    Position + new Vector2Int(-1, -1)
                }
                : new[]
                {
                    Position + new Vector2Int(-1, 0),
                    Position + new Vector2Int(0, 1),
                    Position + new Vector2Int(1, 1),
                    Position + new Vector2Int(1, 0),
                    Position + new Vector2Int(1, -1),
                    Position + new Vector2Int(0, -1)
                };
            
            NeighboursPositions = NeighboursPositions
                .Where(p => p.x >= 0 && p.x < mapSize.x && p.y >= 0 && p.y < mapSize.y)
                .ToArray();
        }

        public override string ToString()
        {
            return $"Hex_{Position.x}_{Position.y}";
        }
    }
}