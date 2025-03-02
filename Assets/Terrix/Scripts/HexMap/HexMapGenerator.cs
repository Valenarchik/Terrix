using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Attributes;
using CustomUtilities.Extensions;
using Terrix.DTO;
using Terrix.Model;
using Terrix.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.HexMap
{
    public class HexMapGenerator : MonoBehaviour
    {
        public class Settings
        {
            public Texture2D Texture2D { get; set; }
            public bool Transpose { get; set; }
            public int ChunkSize { get; set; }
            public List<HexData> HexDatas { get; set; }

            public Settings(Texture2D texture2D, bool transpose, int chunkSize, List<HexData> hexDatas)
            {
                Texture2D = texture2D;
                Transpose = transpose;
                ChunkSize = chunkSize;
                HexDatas = hexDatas;
            }

            public class HexData
            {
                public HexType HexType { get; set; }
                public float Height { get; set; }
                public Tile Tile { get; set; }

                public HexData(HexType hexType, float height, Tile tile)
                {
                    HexType = hexType;
                    Height = height;
                    Tile = tile;
                }
            }
        }

        [SerializeField] private Tilemap tilemap;
        [SerializeField] private HexMapGeneratorSettingsSO initialSettingsSo;

        private Settings settings;

        private void Start()
        {
            if (initialSettingsSo != null)
            {
                GenerateMap(initialSettingsSo.Get());
            }
        }

        [EditorButton]
        private void ManualUpdate()
        {
            if (initialSettingsSo != null)
            {
                GenerateMap(initialSettingsSo.Get());
            }
            else
            {
                Debug.LogWarning("initialSettingsSo is null");
            }
        }

        public Hex[,] GenerateMap(Settings settings)
        {
            ValidateSettings(settings);
            this.settings = AssignSettings(settings);

            var tileData = GenerateTileChangeData();
            tilemap.SetTiles(tileData, true);

            return default;
        }

        private TileChangeData[] GenerateTileChangeData()
        {
            var width = settings.Texture2D.width;
            var height = settings.Texture2D.height;

            var pixels = settings.Texture2D.GetPixels();
            var changeData = new TileChangeData[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var i = y * width + x;
                    var color = pixels[i];
                    var pixelHeight = color.CalculateBrightness();
                    var data = FindData(pixelHeight);
                    var tile = data.Tile;

                    changeData[i] = new TileChangeData
                    {
                        position = settings.Transpose ? new Vector3Int(y, x) : new Vector3Int(x, y),
                        tile = tile,
                        color = Color.white,
                        transform = Matrix4x4.identity
                    };
                }
            }

            return changeData;
        }

        private Settings.HexData FindData(float pixelHeight)
        {
            var nearestValue = settings.HexDatas
                .Select((data, i) => (data, i))
                .OrderBy(data => Math.Abs(data.data.Height - pixelHeight))
                .First();

            return pixelHeight <= nearestValue.data.Height
                ? nearestValue.data
                : settings.HexDatas[nearestValue.i + 1];
        }

        private static void ValidateSettings(Settings settings)
        {
            if (!settings.Texture2D.isReadable)
            {
                throw new Exception("Текстура недоспупня для чтения!");
            }
        }

        private static Settings AssignSettings(Settings settings)
        {
            settings.HexDatas = settings.HexDatas.OrderBy(data => data.Height).ToList();
            settings.HexDatas[^1].Height = 1;
            return settings;
        }
    }
}