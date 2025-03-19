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