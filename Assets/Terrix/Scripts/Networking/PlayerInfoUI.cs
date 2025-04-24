using System;
using Terrix.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Terrix.Networking
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        private Player player;
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        // public void Initialize(string playerName, Color color)
        // {
        //     playerNameText.text = playerName;
        //     rankText.text = RectTransform.GetSiblingIndex().ToString();
        //     rankText.color = color;
        //     playerNameText.color = color;
        //     scoreText.color = color;
        // }
        public void Initialize(Player player, int index)
        {
            this.player = player;
            playerNameText.text = player.PlayerName;
            rankText.text = $"#{index}";
            scoreText.text = "0";
            var color = player.PlayerColor;
            rankText.color = color;
            playerNameText.color = color;
            scoreText.color = color;
        }

        public void UpdateInfo(int order)
        {
            rankText.text = $"#{order}";
            scoreText.text = ((int)player.Country.Population).ToString();
        }
    }
}