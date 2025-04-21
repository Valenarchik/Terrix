using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using CustomUtilities.Attributes;
using Terrix.DTO;
using Terrix.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Settings
{
    [CreateAssetMenu(menuName = "Game settings/Hex map generator settings")]
    public class HexMapGeneratorSettingsSO : ScriptableObject
    {
        public Texture2D Texture2D;
        public int Width;
        public int Height;
        public bool Transpose = true;
        public int ChunkSize;
        public float PerlinMainZoom = 50;
        public NoiseType Noise;
        public float PerlinSecondZoom = 50;
        public int VoronoiGridSize = 10;
        [NamedArray(nameof(HexData.HexType))] public List<HexData> HexDatas;
        [SerializedDictionary] public SerializedDictionary<HexType, Percent> landHexTypes;

        [Serializable]
        public class HexData
        {
            public HexType HexType;
            [Range(0, 1)] public float Height;
        }

        [Serializable]
        public class Percent
        {
            [Range(0, 1)] public float Height;
        }

        public HexMapGenerator.Settings Get()
        {
            return new HexMapGenerator.Settings(Texture2D, Width, Height, Transpose, ChunkSize, GetHexData(),
                landHexTypes, PerlinMainZoom, Noise, PerlinSecondZoom, VoronoiGridSize);
        }

        private List<HexMapGenerator.Settings.HexData> GetHexData()
        {
            return HexDatas
                .Select(data => new HexMapGenerator.Settings.HexData(data.HexType, data.Height))
                .ToList();
        }

        public enum NoiseType
        {
            StaticMap,
            Perlin,
            Voronoi
        }
    }
}