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

namespace Terrix.Map
{
    public class HexMapGenerator: MonoBehaviour
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
        
        public HexMap GenerateMap(Settings settings)
        {
            ValidateSettings(settings);
            this.settings = AssignSettings(settings);

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

                    var hex = new Hex(gameData.CellsStats[data.HexType].HexType, position, grid.CellToWorld(position), map, gameDataProvider, players);
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

                public HexData(HexType hexType, float height)
                {
                    HexType = hexType;
                    Height = height;
                }
            }
        }
    }
}