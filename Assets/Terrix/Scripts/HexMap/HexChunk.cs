// using System.Collections.Generic;
// using UnityEngine;
//
// public class HexChunk : MonoBehaviour
// {
//     public Rect Bounds { get; private set; }
//
//     private MeshFilter meshFilter;
//     private MeshRenderer meshRenderer;
//     private int chunkX, chunkY;
//     private int chunkSize;
//     private float hexWidth, hexHeight;
//
//     public void Initialize(int x, int y, int size, Material material, float hexWidth, float hexHeight)
//     {
//         chunkX = x;
//         chunkY = y;
//         chunkSize = size;
//         this.hexWidth = hexWidth;
//         this.hexHeight = hexHeight;
//
//         meshFilter = gameObject.AddComponent<MeshFilter>();
//         meshRenderer = gameObject.AddComponent<MeshRenderer>();
//         meshRenderer.material = material;
//
//         GenerateChunkMesh();
//         CalculateBounds();
//     }
//
//     private void GenerateChunkMesh()
//     {
//         var mesh = new Mesh { name = $"HexChunkMesh_{chunkX}_{chunkY}" };
//
//         var vertices = new List<Vector3>();
//         var triangles = new List<int>();
//         var uvs = new List<Vector2>();
//
//         for (var x = 0; x < chunkSize; x++)
//         {
//             for (var y = 0; y < chunkSize; y++)
//             {
//                 var center = CalculateHexPosition(x, y);
//                 var vertexIndex = vertices.Count;
//
//                 for (var i = 0; i < 6; i++)
//                 {
//                     var angleDeg = 60 * i;
//                     var angleRad = Mathf.Deg2Rad * angleDeg;
//                     var vertex = center + new Vector3(Mathf.Cos(angleRad) * hexWidth * 0.5f, Mathf.Sin(angleRad) * hexHeight * 0.5f);
//                     vertices.Add(vertex);
//                     uvs.Add(new Vector2((Mathf.Cos(angleRad) + 1) * 0.5f, (Mathf.Sin(angleRad) + 1) * 0.5f));
//                 }
//
//                 for (var i = 0; i < 6; i++)
//                 {
//                     triangles.Add(vertexIndex + (i + 1) % 6);
//                     triangles.Add(vertexIndex);
//                     triangles.Add(vertexIndex + (i + 2) % 6);
//                 }
//             }
//         }
//
//         mesh.SetVertices(vertices);
//         mesh.SetTriangles(triangles, 0);
//         mesh.SetUVs(0, uvs);
//         mesh.RecalculateNormals();
//         mesh.RecalculateBounds();
//
//         meshFilter.mesh = mesh;
//     }
//
//     private Vector3 CalculateHexPosition(int x, int y)
//     {
//         var xPos = (chunkX * chunkSize + x) * (hexWidth * 0.75f);
//         var yPos = (chunkY * chunkSize + y) * hexHeight;
//         if ((chunkX * chunkSize + x) % 2 == 1)
//         {
//             yPos += hexHeight / 2;
//         }
//
//         return new Vector3(xPos, yPos, 0f);
//     }
//
//     private void CalculateBounds()
//     {
//         var min = new Vector2(chunkX * chunkSize * hexWidth * 0.75f, chunkY * chunkSize * hexHeight);
//         var max = new Vector2((chunkX * chunkSize + chunkSize) * hexWidth * 0.75f, (chunkY * chunkSize + chunkSize) * hexHeight + hexHeight * 0.5f);
//         Bounds = new Rect(min, max - min);
//     }
//
//     public void SetVisible(bool isVisible)
//     {
//         gameObject.SetActive(isVisible);
//     }
// }