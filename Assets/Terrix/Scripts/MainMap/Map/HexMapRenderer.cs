using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Terrix.DTO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Map
{
    public class HexMapRenderer: MonoBehaviour
    {
        [SerializeField] private Tilemap[] tileMapLods;
        [SerializeField] private SerializedDictionary<HexType, HexDrawInfo> hexDrawInfos;

        public void RenderMap(HexMap map)
        {
            Clear();
            
            for (var lod = 0; lod < tileMapLods.Length; lod++)
            {
                var changeData = GenerateTileChangeData(lod, map);
                tileMapLods[lod].SetTiles(changeData, true);
            }
        }

        public void Clear()
        {
            foreach (var tilemap in tileMapLods)
            {
                tilemap.ClearAllTiles();
            }
        }

        private TileChangeData[] GenerateTileChangeData(int lod, HexMap map)
        {
            var width = map.Size.x;
            var height = map.Size.y;
            
            var tileChangeData = new TileChangeData[width * height];
            
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var i = y * width + x;
                    var hex = map[x, y, 0];
                    var drawInfo = hexDrawInfos[hex.HexType];

                    tileChangeData[i] = new TileChangeData
                    {
                        position = new Vector3Int(x, y),
                        tile = drawInfo.tileLods[lod],
                        color = Color.white,
                        transform = Matrix4x4.identity
                    };
                }
            }

            return tileChangeData;
        }
        
        [Serializable]
        public class HexDrawInfo
        {
            public TileBase[] tileLods;
        }
    }
}