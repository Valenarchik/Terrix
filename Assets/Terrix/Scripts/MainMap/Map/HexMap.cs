using UnityEngine;

namespace Terrix.Map
{
    public class HexMap
    {
        public Hex[,] Hexes { get; }
        public Vector2Int Size { get; }

        public HexMap(Hex[,] hexes)
        {
            Hexes = hexes;
            Size = new Vector2Int(hexes.GetLength(0), hexes.GetLength(1));
        }
    }
}