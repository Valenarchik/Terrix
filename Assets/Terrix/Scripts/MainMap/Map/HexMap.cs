using System.Linq;
using Terrix.DTO;
using Terrix.Networking;
using UnityEngine;

namespace Terrix.Map
{
    public class HexMap
    {
        public Hex[,,] Hexes { get; }
        public Vector3Int Size { get; }

        public Hex this[int x, int y, int z]
        {
            get => Hexes[x, y, z];
            set => Hexes[x, y, z] = value;
        }

        public Hex this[Vector3Int pos]
        {
            get => this[pos.x, pos.y, pos.z];
            set => this[pos.x, pos.y, pos.z] = value;
        }

        public Hex FindHex(Vector3Int pos)
        {
            if (HasHex(pos))
            {
                return this[pos];
            }

            return null;
        }

        public bool TryGetHex(Vector3Int pos, out Hex hex)
        {
            if (HasHex(pos))
            {
                hex = this[pos];
                return true;
            }

            hex = null;
            return false;
        }

        public bool HasHex(Vector3Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x < Size.x && pos.y < Size.y && pos.z < Size.z;
        }

        public HexMap(Vector3Int size)
        {
            Size = size;
            Hexes = new Hex[size.x, size.y, size.z];
        }

        public HexMap(Hex[,,] hexes)
        {
            Hexes = hexes;
            Size = new Vector3Int(hexes.GetLength(0), hexes.GetLength(1), hexes.GetLength(2));
            foreach (var hex in hexes)
            {
                hex.HexMap = this;
            }
        }

        public int GetNonWaterHexesCount() => Hexes.ToArray().Count(hex => hex.HexType is not HexType.See);
    }
}