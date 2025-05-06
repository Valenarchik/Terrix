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
        
        public Hex FindHex(Vector3Int pos)
        {
            if (HasHex(pos))
            {
                return this[pos];
            }

            return null;
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
    }
}