// using System.Collections.Generic;
// using Priority_Queue;
// using Terrix.DTO;
// using Terrix.Map;
// using UnityEngine;
// using UnityEngine.Tilemaps;
//
// namespace Terrix.Test
// {
//     public class TestStretchingAlgoritm : MonoBehaviour
//     {
//         [SerializeField] private Tilemap tilemap;
//         [SerializeField] private Tile startTile;
//         [SerializeField] private Tile countryTile;
//
//         [Header("Settings")]
//         [SerializeField] private float Alpha = 1.2f;
//         [SerializeField] private float Beta = 0.8f;
//         [SerializeField] private float Gamma = 1.0f;
//         [SerializeField] private float maxPoints = 100;
//
//         private Hex[,] map;
//         private BoundsInt bounds;
//         private Hex start;
//         private HashSet<Hex> country = new();
//
//         private List<TileChangeData> startTiles = new();
//
//         private void Start()
//         {
//             bounds = tilemap.cellBounds;
//             map = new Hex?[bounds.xMax, bounds.yMax];
//
//             for (var x = bounds.xMin; x < bounds.xMax; x++)
//             {
//                 for (var y = bounds.yMin; y < bounds.yMax; y++)
//                 {
//                     for (var z = bounds.zMin; z < bounds.zMax; z++)
//                     {
//                         var cellPosition = new Vector3Int(x, y, z);
//                         var tile = tilemap.GetTile(cellPosition);
//
//                         if (tile != null)
//                         {
//                             var hex = new Hex(HexType.Grasslands, cellPosition);
//                             map[hex.Position.x, hex.Position.y] = hex;
//
//                             if (tile == startTile)
//                             {
//                                 start = hex;
//                             }
//
//                             if (tile == countryTile)
//                             {
//                                 country.Add(hex);
//                             }
//                             
//                             startTiles.Add(new TileChangeData()
//                             {
//                                 position = cellPosition,
//                                 color = Color.white,
//                                 tile = tile,
//                                 transform = Matrix4x4.identity
//                             });
//                         }
//                     }
//                 }
//             }
//         }
//
//         private void Update()
//         {
//             var mousePosition = MapUtilities.GetMousePosition(Input.mousePosition, Camera.main, tilemap);
//
//             if (!tilemap.HasTile(mousePosition))
//             {
//                 return;
//             }
//             
//             
//             var newTiles = StretchBorder(start, map[mousePosition.x, mousePosition.y], country);
//             var tileChangeData = new TileChangeData[newTiles.Count];
//
//             for (var i = 0; i < newTiles.Count; i++)
//             {
//                 tileChangeData[i] = new TileChangeData()
//                 {
//                     position = newTiles[i].Position,
//                     tile = countryTile,
//                     color = Color.white,
//                     transform = Matrix4x4.identity
//                 };
//             }
//             
//             tilemap.SetTiles(startTiles.ToArray(), true);
//             tilemap.SetTiles(tileChangeData, true);
//         }
//
//         private List<Hex> StretchBorder(
//             Hex start,
//             Hex end,
//             HashSet<Hex> country)
//         {
//             var result = new List<Hex>();
//             var visited = new HashSet<Hex>();
//             var priorityQueue = new SimplePriorityQueue<Hex, float>();
//             priorityQueue.Enqueue(start, 0);
//
//             var remainingPoints = maxPoints;
//             var targetReached = false;
//
//             while (priorityQueue.Count > 0 && remainingPoints > 0 && !targetReached)
//             {
//                 priorityQueue.TryDequeue(out var cell);
//                 if (visited.Contains(cell) || country.Contains(cell))
//                 {
//                     continue;
//                 }
//
//                 var cellCost = 15;
//                 if (cellCost > remainingPoints)
//                 {
//                     continue;
//                 }
//
//                 result.Add(cell);
//
//                 if (end.Equals(cell))
//                 {
//                     targetReached = true;
//                 }
//                 
//                 remainingPoints -= cellCost;
//                 visited.Add(cell);
//                 
//                 
//                 foreach (var neighborPosition in cell.NeighboursPositions)
//                 {
//                     if (!tilemap.HasTile(neighborPosition))
//                     {
//                         continue;
//                     }
//
//                     var neighbor = map[neighborPosition.x, neighborPosition.y];
//
//                     if (country.Contains(neighbor) || visited.Contains(neighbor))
//                     {
//                         continue;
//                     }
//
//                     var priority = CalculatePriority(start, end, neighbor, 15);
//                     priorityQueue.Enqueue(neighbor, -priority);
//                 }
//             }
//
//             return result;
//         }
//
//         private float CalculatePriority(Hex start, Hex end, Hex current, float cost)
//         {
//             Vector3 direction = end.Position - start.Position;
//             Vector3 delta = current.Position - start.Position;
//             
//             var projection = Vector3.Dot(delta.normalized, direction.normalized);
//             
//             var distance = delta.magnitude;
//             var distanceFactor = 1f / (1f + distance);
//
//             return (projection * Alpha) + (distanceFactor * Beta) + (cost * Gamma);
//         }
//     }
// }