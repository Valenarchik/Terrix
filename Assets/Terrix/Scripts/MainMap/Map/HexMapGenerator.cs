using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Extensions;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Settings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Terrix.Map
{
    public class HexMapGenerator : MonoBehaviour
    {
        [SerializeField] private Grid grid;

        private IGameDataProvider gameDataProvider;
        private IPlayersProvider players;

        private Settings settings;

        public void Initialize([NotNull] IGameDataProvider gameData, [NotNull] IPlayersProvider players)
        {
            this.gameDataProvider = gameData ?? throw new ArgumentNullException(nameof(gameData));
            this.players = players ?? throw new ArgumentNullException(nameof(players));
        }

        public void InitializeMocks()
        {
            this.gameDataProvider = new GameDataProvider();
            this.players = new PlayersProvider(Enumerable.Empty<Player>());
        }

        public HexMap GenerateMap(Settings initSettings)
        {
            ValidateSettings(initSettings);
            this.settings = AssignSettings(initSettings);

            return GenerateUniversalMap();
        }

        private HexMap GenerateStaticMap()
        {
            var gameData = gameDataProvider.Get();
            var texture2DWidth = settings.Texture2D.width;
            var texture2DHeight = settings.Texture2D.height;
            var pixels = settings.Texture2D.GetPixels();
            var mapSize = settings.Transpose
                ? new Vector3Int(texture2DHeight, texture2DWidth, 1)
                : new Vector3Int(texture2DWidth, texture2DHeight, 1);
            var map = new HexMap(mapSize);
            for (var y = 0; y < texture2DHeight; y++)
            {
                for (var x = 0; x < texture2DWidth; x++)
                {
                    var i = y * texture2DWidth + x;
                    var color = pixels[i];
                    var pixelHeight = color.CalculateBrightness();
                    var data = FindData(pixelHeight);
                    var position = settings.Transpose ? new Vector3Int(y, x, 0) : new Vector3Int(x, y, 0);
                    var hex = new Hex(gameData.CellsStats[data.HexType].HexType, position, grid.CellToWorld(position),
                        map, gameDataProvider, players);
                    map[hex.Position.x, hex.Position.y, 0] = hex;
                }
            }

            return map;
        }

        private HexMap GenerateUniversalMap()
        {
            if (settings.NoiseType is HexMapGeneratorSettingsSO.NoiseType.StaticMap)
            {
                return GenerateStaticMap();
            }

            var gameData = gameDataProvider.Get();
            var mapSize = new Vector3Int(settings.Width, settings.Height, 1);

            var map = new HexMap(mapSize);
            var matrix = GeneratePerlinMatrix((Vector2Int)mapSize, settings.PerlinMainZoom);
            var landDictionary = new Dictionary<HexType, float>();
            foreach (var landHexType in settings.LandHexTypes)
            {
                var borders = FindHexTypeHeights(landHexType.Key);
                landDictionary.Add(landHexType.Key, (borders.Item1 + borders.Item2) / 2);
            }

            switch (settings.NoiseType)
            {
                case HexMapGeneratorSettingsSO.NoiseType.Perlin:
                    PutPerlinOnMatrix(matrix, new Vector2Int(settings.Width, settings.Height),
                        settings.PerlinSecondZoom,
                        FindHexTypeHeights(HexType.Farmlands).Item1,
                        FindHexTypeHeights(HexType.Forest).Item2,
                        landDictionary);
                    break;
                case HexMapGeneratorSettingsSO.NoiseType.Voronoi:
                    PutVoronoiOnMatrix(matrix, settings.VoronoiGridSize, FindHexTypeHeights(HexType.Farmlands).Item1,
                        FindHexTypeHeights(HexType.Forest).Item2);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (var y = 0; y < settings.Height; y++)
            {
                for (var x = 0; x < settings.Width; x++)
                {
                    var pixelHeight = matrix[x, y];
                    pixelHeight = Mathf.Clamp(pixelHeight, 0, 1);
                    var data = FindData(pixelHeight);
                    var position = new Vector3Int(x, y, 0);

                    var hex = new Hex(gameData.CellsStats[data.HexType].HexType, position,
                        grid.CellToWorld(position), map, gameDataProvider, players);
                    map[hex.Position.x, hex.Position.y, 0] = hex;
                }
            }

            return map;
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

        public Texture2D GeneratePerlinTexture(Vector2Int size, float zoom)
        {
            float seedX = Random.Range(-100000f, 100000f);
            float seedY = Random.Range(-100000f, 100000f);
            var result = new Texture2D(size.x, size.y);
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var height = GetPerlinHeight(x, seedX, y, seedY, zoom);
                    result.SetPixel(x, y, new Color(height, height, height, 1));
                }
            }

            return result;
        }

        public float[,] GeneratePerlinMatrix(Vector2Int size, float zoom)
        {
            float seedX = Random.Range(-100000f, 100000f);
            float seedY = Random.Range(-100000f, 100000f);
            var result = new float[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var height = GetPerlinHeight(x, seedX, y, seedY, zoom);
                    result[x, y] = height;
                }
            }

            return result;
        }

        public void PutPerlinOnMatrix(float[,] matrix, Vector2Int size, float zoom,
            float minValue, float maxValue, Dictionary<HexType, float> landDictionary)
        {
            var secondMatrix = GeneratePerlinMatrix(size, zoom);
            var landHexValues = settings.LandHexTypes.Values.ToList();
            var landHexKeys = settings.LandHexTypes.Keys.ToList();
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    if (matrix[x, y] >= minValue && matrix[x, y] < maxValue)
                    {
                        var newValue = secondMatrix[x, y];
                        for (int i = 0; i < settings.LandHexTypes.Count; i++)
                        {
                            if (newValue <= landHexValues[i].Height)
                            {
                                matrix[x, y] = landDictionary[landHexKeys[i]];
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void PutVoronoiOnMatrix(float[,] matrix, int gridSize,
            float minValue, float maxValue)
        {
            int pixelsPerCell = matrix.GetLength(0) / gridSize;
            var allNeededPixels = new List<Vector2Int>();
            var points = new List<Vector2Int>();
            var pointsHeights = new Dictionary<Vector2Int, float>();
            for (int x = 0; x < gridSize + 1; x++)
            {
                for (int y = 0; y < gridSize + 1; y++)
                {
                    var list = new List<Vector2Int>();
                    for (int i = 0; i < pixelsPerCell; i++)
                    {
                        var absoluteX = x * pixelsPerCell + i;
                        if (absoluteX >= matrix.GetLength(0))
                        {
                            continue;
                        }

                        for (int j = 0; j < pixelsPerCell; j++)
                        {
                            var absoluteY = y * pixelsPerCell + j;
                            if (absoluteY >= matrix.GetLength(1))
                            {
                                continue;
                            }

                            var value = matrix[absoluteX, absoluteY];
                            if (value >= minValue && value < maxValue)
                            {
                                var vector = new Vector2Int(absoluteX, absoluteY);
                                list.Add(vector);
                                allNeededPixels.Add(vector);
                            }
                        }
                    }

                    if (!list.Any())
                    {
                        continue;
                    }

                    var point = list[Random.Range(0, list.Count)];
                    points.Add(point);
                    pointsHeights.Add(point, Random.Range(minValue, maxValue));
                }
            }

            foreach (var pixel in allNeededPixels)
            {
                var nearestDistance = Mathf.Infinity;
                var nearestPoint = new Vector2Int(0, 0);
                foreach (var point in points)
                {
                    var distance = Vector2Int.Distance(point, pixel);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPoint = point;
                    }
                }

                matrix[pixel.x, pixel.y] = pointsHeights[nearestPoint];
            }

            // Попытка сгладить углы (неудачная)
            // for (int x = 0; x < matrix.GetLength(0); x++)
            // {
            //     for (int y = 0; y < matrix.GetLength(1); y++)
            //     {
            //         if (!allNeededPixels.Contains(new Vector2Int(x, y)))
            //         {
            //             continue;
            //         }
            //
            //         var landTypes = new Dictionary<HexType, int>();
            //
            //         for (int i = -1; i < 2; i++)
            //         {
            //             var newX = x + i;
            //             if (newX < 0 || newX >= matrix.GetLength(0))
            //             {
            //                 continue;
            //             }
            //
            //             for (int j = -1; j < 2; j++)
            //             {
            //                 var newY = y + j;
            //                 if (newY < 0 || newY >= matrix.GetLength(1))
            //                 {
            //                     continue;
            //                 }
            //
            //                 var vector = new Vector2Int(newX, newY);
            //                 if (allNeededPixels.Contains(vector))
            //                 {
            //                     var height = allNeededPixelsDictionary[vector];
            //                     // }
            //                     // if (pointsHeights.TryGetValue(new Vector2Int(newX, newY), out var height))
            //                     // {
            //                     var type = GetLandHexType(height);
            //                     if (!landTypes.TryAdd(type, 1))
            //                     {
            //                         landTypes[type]++;
            //                     }
            //                 }
            //             }
            //         }
            //
            //         var finalType = landTypes.OrderByDescending(pair => pair.Value).First().Key;
            //         matrix[x, y] = landHeightDictionary[finalType];
            //     }
            // }
        }

        public float GetPerlinHeight(int x, float seedX, int y, float seedY, float zoom)
        {
            return Mathf.PerlinNoise((x + seedX) / zoom, (y + seedY) / zoom);
        }

        private Tuple<float, float> FindHexTypeHeights(HexType hexType)
        {
            var index = settings.HexDatas.FindIndex(hex => hex.HexType == hexType);
            float minHeight;
            float maxHeight = settings.HexDatas[index].Height;
            if (index == 0)
            {
                minHeight = 0;
            }
            else
            {
                minHeight = settings.HexDatas[index - 1].Height;
            }

            return new Tuple<float, float>(minHeight, maxHeight);
        }

        public class Settings
        {
            public Texture2D Texture2D { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public bool Transpose { get; set; }
            public int ChunkSize { get; set; }
            public float PerlinMainZoom { get; set; }
            public HexMapGeneratorSettingsSO.NoiseType NoiseType { get; set; }
            public float PerlinSecondZoom { get; set; }
            public int VoronoiGridSize { get; set; }


            public List<HexData> HexDatas { get; set; }
            public Dictionary<HexType, HexMapGeneratorSettingsSO.Percent> LandHexTypes { get; set; }


            public Settings(Texture2D texture2D, int width, int height, bool transpose, int chunkSize,
                List<HexData> hexDatas, Dictionary<HexType, HexMapGeneratorSettingsSO.Percent> landHexTypes,
                float perlinMainZoom, HexMapGeneratorSettingsSO.NoiseType noiseType, float perlinSecondZoom,
                int voronoiGridSize)
            {
                Texture2D = texture2D;
                Width = width;
                Height = height;
                Transpose = transpose;
                ChunkSize = chunkSize;
                HexDatas = hexDatas;
                LandHexTypes = landHexTypes;
                PerlinMainZoom = perlinMainZoom;
                NoiseType = noiseType;
                PerlinSecondZoom = perlinSecondZoom;
                VoronoiGridSize = voronoiGridSize;
            }

            public class HexData
            {
                public HexType HexType { get; set; }
                public float Height { get; set; }

                public HexData(HexType hexType, float height)
                {
                    HexType = hexType;
                    Height = height;
                }
            }
        }
    }
}