using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Attributes;
using Terrix.DTO;
using Terrix.HexMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Settings
{
    [CreateAssetMenu(menuName = "Game settings/Hex map generator settings")]
    public class HexMapGeneratorSettingsSO : ScriptableObject
    {
        public Texture2D Texture2D;
        public bool Transpose = true;
        public int ChunkSize;
        [NamedArray(nameof(HexData.HexType))] public List<HexData> HexDatas;

        [Serializable]
        public class HexData
        {
            public HexType HexType;
            [Range(0, 1)] public float Height;
            public Tile Tile;
        }

        public HexMapGenerator.Settings Get()
        {
            return new HexMapGenerator.Settings(Texture2D, Transpose, ChunkSize, GetHexData());
        }

        private List<HexMapGenerator.Settings.HexData> GetHexData()
        {
            return HexDatas
                .Select(data => new HexMapGenerator.Settings.HexData(data.HexType, data.Height, data.Tile))
                .ToList();
        }
    }
}