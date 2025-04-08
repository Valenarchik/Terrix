using System;
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
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(string playerName, Color color)
        {
            playerNameText.text = playerName;
            rankText.text = RectTransform.GetSiblingIndex().ToString();
            rankText.color = color;
            playerNameText.color = color;
            scoreText.color = color;
        }

        public void UpdateInfo(float points)
        {
            rankText.text = $"#{RectTransform.GetSiblingIndex() + 1}";
            scoreText.text = points.ToString();
        }
    }
}