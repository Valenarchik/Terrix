using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Map
{
    public static class MapUtilities
    {
        public static Vector3Int[] GetHexNeighborsPositions(Vector3Int pos)
        {
            var neighboursPositions = pos.y % 2 == 0
                ? new[]
                {
                    pos + new Vector3Int(-1, 0, 0),
                    pos + new Vector3Int(-1, 1, 0),
                    pos + new Vector3Int(0, 1, 0),
                    pos + new Vector3Int(1, 0, 0),
                    pos + new Vector3Int(0, -1, 0),
                    pos + new Vector3Int(-1, -1, 0)
                }
                : new[]
                {
                    pos + new Vector3Int(-1, 0, 0),
                    pos + new Vector3Int(0, 1, 0),
                    pos + new Vector3Int(1, 1, 0),
                    pos + new Vector3Int(1, 0, 0),
                    pos + new Vector3Int(1, -1, 0),
                    pos + new Vector3Int(0, -1, 0)
                };

            return neighboursPositions;
        }

        public static Vector3Int GetMousePosition(Vector3 screenPosition, Camera camera, Tilemap tilemap)
        {
            var worldPosition = camera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;
            return tilemap.WorldToCell(worldPosition);
        }
    }
}