using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Attributes;
using CustomUtilities.Extensions;
using Terrix.DTO;
using Terrix.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Map
{
    public class HexMapGenerator : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private HexMapGeneratorSettingsSO initialSettingsSo;

        private IGameDataProvider gameDataProvider = new GameDataProvider();
        private Settings settings;

        public HexMapGeneratorSettingsSO DefaultSettingsSo => initialSettingsSo;

        [EditorButton]
        private void UpdateMap()
        {
            ClearMap();
            
            if (initialSettingsSo != null)
            {
                GenerateMap(initialSettingsSo.Get());
            }
            else
            {
                Debug.LogWarning("initialSettingsSo is null");
            }
        }

        [EditorButton]
        private void ClearMap()
        {
            
            tilemap.ClearAllTiles();
        }

        // Разделить ответсвенность по созданию модели и визуализации карты?
        // + можно будет разделить ответственность, модель создается на сервере, а визуал на криенте.
        public HexMap GenerateMap(Settings initSettings)
        {
            ValidateSettings(initSettings);
            this.settings = AssignSettings(initSettings);

            GenerateData(out var tileData, out var map);
            tilemap.SetTiles(tileData, true);

            return new HexMap(map);
        }

        private void GenerateData(out TileChangeData[] tileChangeData, out Hex[,] map)
        {
            var gameData = gameDataProvider.Get();
            var texture2DWidth = settings.Texture2D.width;
            var texture2DHeight = settings.Texture2D.height;

            var pixels = settings.Texture2D.GetPixels();
            tileChangeData = new TileChangeData[texture2DWidth * texture2DHeight];

            var mapSize = settings.Transpose
                ? new Vector2Int(texture2DHeight, texture2DWidth)
                : new Vector2Int(texture2DWidth, texture2DHeight);

            map = new Hex[mapSize.x, mapSize.y];

            for (var y = 0; y < texture2DHeight; y++)
            {
                for (var x = 0; x < texture2DWidth; x++)
                {
                    var i = y * texture2DWidth + x;
                    var color = pixels[i];
                    var pixelHeight = color.CalculateBrightness();
                    var data = FindData(pixelHeight);
                    var position = settings.Transpose ? new Vector2Int(y, x) : new Vector2Int(x, y);

                    tileChangeData[i] = new TileChangeData
                    {
                        position = new Vector3Int(position.x, position.y),
                        tile = data.Tile,
                        color = Color.white,
                        transform = Matrix4x4.identity
                    };

                    var hex = new Hex(gameData.CellsStats[data.HexType], position, mapSize);
                    map[hex.Position.x, hex.Position.y] = hex;
                }
            }
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
                throw new Exception("Текстура недоступна для чтения!");
            }
        }

        private static Settings AssignSettings(Settings settings)
        {
            settings.HexDatas = settings.HexDatas.OrderBy(data => data.Height).ToList();
            settings.HexDatas[^1].Height = 1;
            return settings;
        }

        public class Settings
        {
            public Texture2D Texture2D { get; set; }
            public bool Transpose { get; set; }
            public List<HexData> HexDatas { get; set; }

            public Settings(Texture2D texture2D, bool transpose, List<HexData> hexDatas)
            {
                Texture2D = texture2D;
                Transpose = transpose;
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
    }
}