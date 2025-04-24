using Terrix.Map;
using UnityEngine;

namespace Terrix.Network.DTO
{
    public struct SimplifiedCellChangeData
    {
        public Vector3Int Position { get; set; }
        public Country.UpdateCellMode Mode { get; set; }

        public SimplifiedCellChangeData(Vector3Int positon, Country.UpdateCellMode mode)
        {
            Position = positon;
            Mode = mode;
        }
    }
}