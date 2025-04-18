using System;
using System.Linq;
using JetBrains.Annotations;
using CustomUtilities.Attributes;
using NUnit.Framework.Constraints;
using Terrix.DTO;
using Terrix.Map;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Terrix.Visual
{
    public class CountryDrawer : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap zoneTilemap;
        [SerializeField] private Tile zoneTile;
        [SerializeField] private TextMeshPro playerNameText;
        [SerializeField] private TextMeshPro playerScoreText;
        [SerializeField] private RectTransform playerInfoHolder;

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

            var tilemapRenderer = zoneTilemap.GetComponent<TilemapRenderer>();
            tilemapRenderer.sharedMaterial = zoneMaterial;
            tilemapRenderer.sortingOrder = settings.SortingOrder;
            // playerInfoHolder.gameObject.SetActive(true);
            playerNameText.text = settings.PlayerName;
        }

        public void UpdateZone([NotNull] Country.UpdateCellsData data, float? score)
        {
            Debug.Log($"Changed {data.PlayerId}");
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
            zoneTilemap.CompressBounds();
            var cellBounds = zoneTilemap.cellBounds;
            // playerNameText.rectTransform.position = new Vector3(cellBounds.center.y * grid.cellSize.y * 0.75f,
            //     cellBounds.center.x * grid.cellSize.x);
            // playerNameText.rectTransform.sizeDelta = new Vector2(cellBounds.size.x, cellBounds.size.y);
            if (score is null || playerId == -1)
            {
                return;
            }

            playerInfoHolder.gameObject.SetActive(true);
            playerInfoHolder.position = new Vector3(cellBounds.center.y * grid.cellSize.y * 0.75f,
                cellBounds.center.x * grid.cellSize.x);
            playerInfoHolder.sizeDelta = new Vector2(cellBounds.size.x, cellBounds.size.y);
        }

        public void UpdateScore(float score)
        {
            playerScoreText.text = score.ToString();
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
            public int SortingOrder { get; }
            public string PlayerName { get; }

            public Settings(int playerId, Material zoneMaterial, int sortingOrder, string playerName)
            {
                PlayerId = playerId;
                ZoneMaterial = zoneMaterial;
                SortingOrder = sortingOrder;
                PlayerName = playerName;
            }
        }
    }
}