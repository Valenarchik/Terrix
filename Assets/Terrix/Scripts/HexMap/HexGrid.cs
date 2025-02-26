// using UnityEngine;
//
// public class HexGrid : MonoBehaviour
// {
//     [SerializeField] private Material hexMaterial;
//     [SerializeField] private int mapWidth = 500;
//     [SerializeField] private int mapHeight = 500;
//     [SerializeField] private int chunkSize = 50;
//     [SerializeField] private float hexSize = 1f;
//     [SerializeField] private Camera mainCamera;
//     [SerializeField] private float visibilityBuffer = 1.2f;
//
//     private float hexWidth;
//     private float hexHeight;
//     private int chunksX;
//     private int chunksY;
//     private HexChunk[,] chunks;
//
//     private void Start()
//     {
//         hexHeight = Mathf.Sqrt(3f) * hexSize;
//         hexWidth = 2f * hexSize;
//
//         chunksX = Mathf.CeilToInt(mapWidth / (float)chunkSize);
//         chunksY = Mathf.CeilToInt(mapHeight / (float)chunkSize);
//         chunks = new HexChunk[chunksX, chunksY];
//
//         GenerateGrid();
//     }
//
//     private void Update()
//     {
//         UpdateChunksVisibility();
//     }
//
//     private void GenerateGrid()
//     {
//         for (var chunkX = 0; chunkX < chunksX; chunkX++)
//         {
//             for (var chunkY = 0; chunkY < chunksY; chunkY++)
//             {
//                 var chunkObj = new GameObject($"Chunk_{chunkX}_{chunkY}");
//                 chunkObj.transform.parent = transform;
//
//                 var chunk = chunkObj.AddComponent<HexChunk>();
//                 chunk.Initialize(chunkX, chunkY, chunkSize, hexMaterial, hexWidth, hexHeight);
//                 chunks[chunkX, chunkY] = chunk;
//             }
//         }
//     }
//
//     private void UpdateChunksVisibility()
//     {
//         var cameraBounds = GetCameraBoundsWithBuffer();
//
//         for (var chunkX = 0; chunkX < chunksX; chunkX++)
//         {
//             for (var chunkY = 0; chunkY < chunksY; chunkY++)
//             {
//                 var isVisible = cameraBounds.Overlaps(chunks[chunkX, chunkY].Bounds);
//                 chunks[chunkX, chunkY].SetVisible(isVisible);
//             }
//         }
//     }
//
//     private Rect GetCameraBoundsWithBuffer()
//     {
//         var min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, mainCamera.nearClipPlane));
//         var max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, mainCamera.nearClipPlane));
//         var size = (max - min) * visibilityBuffer;
//         return new Rect(min, size);
//     }
// }
