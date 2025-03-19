using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Map
{
    public static class MapUtilities
    {
        public static bool AvailablePosition(Vector2Int pos, Vector2Int mapSize)
        {
            return pos.x >= 0 && pos.x < mapSize.x && pos.y >= 0 && pos.y < mapSize.y;
        }

        public static Vector2Int[] GetHexNeighborsPositions(Vector2Int pos, Vector2Int mapSize)
        {
            var neighboursPositions = pos.y % 2 == 0
                ? new[]
                {
                    pos + new Vector2Int(-1, 0),
                    pos + new Vector2Int(-1, 1),
                    pos + new Vector2Int(0, 1),
                    pos + new Vector2Int(1, 0),
                    pos + new Vector2Int(0, -1),
                    pos + new Vector2Int(-1, -1)
                }
                : new[]
                {
                    pos + new Vector2Int(-1, 0),
                    pos + new Vector2Int(0, 1),
                    pos + new Vector2Int(1, 1),
                    pos + new Vector2Int(1, 0),
                    pos + new Vector2Int(1, -1),
                    pos + new Vector2Int(0, -1)
                };

            return neighboursPositions
                .Where(p => AvailablePosition(p, mapSize))
                .ToArray();
        }

        public static Vector2Int GetHexPosition(Vector2 screenPosition, Camera camera, Tilemap tilemap)
        {
            var worldPosition = camera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;
            return (Vector2Int) tilemap.WorldToCell(worldPosition);
        }
    }
}