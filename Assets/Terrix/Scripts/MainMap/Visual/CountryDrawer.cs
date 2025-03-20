using System;
using JetBrains.Annotations;
using CustomUtilities.Attributes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Visual
{
    public class CountryDrawer: MonoBehaviour
    {
        [SerializeField] private Tilemap zoneTilemap;
        [SerializeField] private Tile zoneTile;
        
        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private int countryId;
        
        private Material zoneMaterial;

        public void Initialize([NotNull] Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            gameObject.name = $"Country_{countryId}";
            
            this.countryId = settings.CountryId;
            this.zoneMaterial = settings.ZoneMaterial;

            zoneTilemap.GetComponent<TilemapRenderer>().sharedMaterial = zoneMaterial;
        }

        public void UpdateZone([NotNull] ZoneUpdateData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.CountryId != countryId)
            {
                throw new InvalidOperationException($"{nameof(CountryDrawer)}.{nameof(UpdateZone)} | Не верно указан id!");
            }

            var changeData = GenerateData(data);
            zoneTilemap.SetTiles(changeData, true);
        }

        private TileChangeData[] GenerateData(ZoneUpdateData data)
        {
            var changeData = new TileChangeData[data.AddedTiles.Length + data.RemovedTiles.Length];

            for (var i = 0; i < changeData.Length; i++)
            {
                if (i < data.AddedTiles.Length)
                {
                    changeData[i] = new TileChangeData
                    {
                        position = (Vector3Int) data.AddedTiles[i],
                        tile = zoneTile,
                        color = Color.white,
                        transform = Matrix4x4.identity
                    };
                }
                else
                {
                    changeData[i] = new TileChangeData
                    {
                        position = (Vector3Int) data.AddedTiles[i - data.AddedTiles.Length],
                        tile = null,
                        color = Color.white,
                        transform = Matrix4x4.identity
                    };
                }
            }

            return changeData;
        }

        public class Settings
        {
            public int CountryId { get;}
            public Material ZoneMaterial { get; }
            
            public Settings(int countryId, Material zoneMaterial)
            {
                CountryId = countryId;
                ZoneMaterial = zoneMaterial;
            }
        }
        
        public class ZoneUpdateData
        {
            public int CountryId { get; set; }
            public Vector2Int[] AddedTiles { get; set; } = Array.Empty<Vector2Int>();
            public Vector2Int[] RemovedTiles { get; set; } = Array.Empty<Vector2Int>();
        }
    }
}