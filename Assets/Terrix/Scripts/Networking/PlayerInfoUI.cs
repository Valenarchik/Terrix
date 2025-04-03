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

        public void Initialize(string playerName)
        {
            playerNameText.text = playerName;
            rankText.text = RectTransform.GetSiblingIndex().ToString();
        }

        public void UpdateInfo(float points)
        {
            rankText.text = RectTransform.GetSiblingIndex().ToString();
            scoreText.text = points.ToString();
        }
    }
}