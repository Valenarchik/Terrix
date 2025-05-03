using System;
using Terrix.Entities;
using Terrix.Game.GameRules;
using TMPro;
using UnityEngine;

namespace Terrix.Networking
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI mapPercentageText;
        [SerializeField] private TextMeshProUGUI populationGainText;
        [SerializeField] private TextMeshProUGUI populationText;
        private Player player;
        private int NonWaterHexesCount;
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(Player player, int index, int nonWaterHexesCount)
        {
            this.player = player;
            playerNameText.text = player.PlayerName;
            rankText.text = $"#{index}";
            populationText.text = "0";
            populationGainText.text = "0";
            mapPercentageText.text = "0%";
            var color = player.PlayerColor;
            rankText.color = color;
            playerNameText.color = color;
            mapPercentageText.color = color;
            populationGainText.color = color;
            populationText.color = color;
            NonWaterHexesCount = nonWaterHexesCount;
        }

        public void UpdateInfo(int order)
        {
            rankText.text = $"#{order}";
            mapPercentageText.text =
                $"{Math.Round((float)player.Country.TotalCellsCount / NonWaterHexesCount, 4) * 100}%";
            populationGainText.text = ((int)player.Country.GetIncome()).ToString();
            populationText.text = ((int)player.Country.Population).ToString();
        }

        public void Expand()
        {
            mapPercentageText.gameObject.SetActive(true);
            populationGainText.gameObject.SetActive(true);
        }

        public void Compress()
        {
            mapPercentageText.gameObject.SetActive(false);
            populationGainText.gameObject.SetActive(false);
        }
    }
}