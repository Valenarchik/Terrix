using FishNet.CodeGenerating;
using FishNet.Serializing;
using UnityEngine;

namespace Terrix.Map
{
    // [UseGlobalCustomSerializer]
    public class HexMap
    {
        public Hex[,] Hexes { get; }
        public Vector2Int Size { get; }

        public Hex this[int x, int y]
        {
            get => Hexes[x, y];
            set => Hexes[x, y] = value;
        }
        
        public Hex this[Vector2Int pos]
        {
            get => Hexes[pos.x, pos.y];
            set => Hexes[pos.x, pos.y] = value;
        }

        public HexMap(Hex[,] hexes)
        {
            Hexes = hexes;
            Size = new Vector2Int(hexes.GetLength(0), hexes.GetLength(1));
        }

        public HexMap(Hex[,] hexes, Vector2Int size)
        {
            Hexes = hexes;
            Size = size;
        }
    }
}