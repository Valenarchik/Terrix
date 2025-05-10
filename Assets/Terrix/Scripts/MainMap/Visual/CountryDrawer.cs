using System;
using JetBrains.Annotations;
using CustomUtilities.Attributes;
using Terrix.Map;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Visual
{
    public class CountryDrawer : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap zoneTilemap;
        [SerializeField] private TextMeshPro playerNameText;
        [SerializeField] private TextMeshPro playerScoreText;
        [SerializeField] private RectTransform playerInfoHolder;
        [SerializeField] private TileBase zoneTile;

        [Header("Debug")]
        [SerializeField, ReadOnlyInspector] private int playerId = int.MinValue;

        private Material zoneMaterial;

        private void Update()
        {
            //По сути костыль
            if (Math.Abs(playerNameText.fontSize - playerScoreText.fontSize) > 0.1)
            {
                playerNameText.fontSize = playerScoreText.fontSize;
            }
        }

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
            playerNameText.text = settings.PlayerName;
            var countryColor = settings.ZoneMaterial.GetColor(Shader.PropertyToID("_BaseColor"));
            if (countryColor.grayscale >= 0.5f)
            {
                playerNameText.color = Color.black;
                playerScoreText.color = Color.black;
            }
            else
            {
                playerNameText.color = Color.white;
                playerScoreText.color = Color.white;
            }
        }


        public void UpdateZone([NotNull] Country.UpdateCellsData data, float? score)
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
            if (score is null || playerId == -1)
            {
                return;
            }

            UpdateInfoHolder();
        }

        private void UpdateInfoHolder()
        {
            zoneTilemap.CompressBounds();
            var cellBounds = zoneTilemap.cellBounds;
            playerInfoHolder.gameObject.SetActive(true);
            playerInfoHolder.position = new Vector3(cellBounds.center.y * grid.cellSize.y * 0.75f,
                cellBounds.center.x * grid.cellSize.x);
            playerInfoHolder.sizeDelta = new Vector2(cellBounds.size.x, cellBounds.size.y);
            playerNameText.fontSize = playerScoreText.fontSize;
        }

        public void UpdateScore(float score)
        {
            playerScoreText.text = ((int)score).ToString();
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