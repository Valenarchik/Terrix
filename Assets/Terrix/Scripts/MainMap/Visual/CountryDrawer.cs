using System;
using JetBrains.Annotations;
using CustomUtilities.Attributes;
using Terrix.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Visual
{
    public class CountryDrawer : MonoBehaviour
    {
        [SerializeField] private Tilemap zoneTilemap;
        [SerializeField] private Tile zoneTile;

        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private int playerId = int.MinValue;

        private Material zoneMaterial;

        public void Initialize([NotNull] Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.playerId = settings.PlayerId;
            gameObject.name = $"Country_{playerId}";

            this.zoneMaterial = settings.ZoneMaterial;

            zoneTilemap.GetComponent<TilemapRenderer>().sharedMaterial = zoneMaterial;
        }

        public void UpdateZone([NotNull] Country.UpdateCellsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.PlayerId != playerId)
            {
                throw new InvalidOperationException(
                    $"{nameof(CountryDrawer)}.{nameof(UpdateZone)} | Не верно указан id!");
            }

            var changeData = GenerateData(data);
            zoneTilemap.SetTiles(changeData, true);
        }

        private TileChangeData[] GenerateData(Country.UpdateCellsData data)
        {
            var changeData = new TileChangeData[data.ChangeData.Length];

            for (var i = 0; i < changeData.Length; i++)
            {
                var hex = data.ChangeData[i].Hex;
                changeData[i] = data.ChangeData[i].Mode switch
                {
                    Country.UpdateCellMode.Add => new TileChangeData
                    {
                        position = hex.Position,
                        tile = zoneTile,
                        color = Color.white,
                        transform = Matrix4x4.identity
                    },
                    Country.UpdateCellMode.Remove => new TileChangeData
                    {
                        position = hex.Position,
                        tile = null,
                        color = Color.white, 
                        transform = Matrix4x4.identity
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return changeData;
        }

        public class Settings
        {
            public int PlayerId { get; }
            public Material ZoneMaterial { get; }

            public Settings(int playerId, Material zoneMaterial)
            {
                PlayerId = playerId;
                ZoneMaterial = zoneMaterial;
            }
        }
    }
}