using System.Linq;
using Terrix.DTO;
using UnityEngine;

namespace Terrix.Map
{
    public class HexMap
    {
        public Hex[,,] Hexes { get; }
        public Hex[] CanCaptureHexes { get; }
        public Vector3Int Size { get; }

        public Hex this[int x, int y, int z]
        {
            get => Hexes[x, y, z];
            set => Hexes[x, y, z] = value;
        }

        public Hex this[Vector3Int pos]
        {
            get => Hexes[pos.x, pos.y, pos.z];
            set => Hexes[pos.x, pos.y, pos.z] = value;
        }

        public bool HasHex(Vector3Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x < Size.x && pos.y < Size.y && pos.z < Size.z;
        }

        public HexMap(Hex[,,] hexes, GameData gameData)
        {
            Hexes = hexes;
            Size = new Vector3Int(hexes.GetLength(0), hexes.GetLength(1), hexes.GetLength(2));
            CanCaptureHexes = Hexes.Cast<Hex>().Where(hex => hex.GetHexData(gameData).CanCapture).ToArray();
        }
    }
}