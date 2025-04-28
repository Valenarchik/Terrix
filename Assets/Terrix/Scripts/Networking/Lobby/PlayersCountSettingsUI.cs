using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terrix.Networking
{
    public class PlayersCountSettingsUI : MonoBehaviour
    {
        [SerializeField] private int minCount;
        [SerializeField] private int maxCount;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI minCountText;
        [SerializeField] private TextMeshProUGUI maxCountText;
        [SerializeField] private TextMeshProUGUI currentCountText;
        [SerializeField] private string prefix;
        public int CurrentValue { get; private set; }

        private void Start()
        {
            minCountText.text = minCount.ToString();
            maxCountText.text = maxCount.ToString();
            slider.minValue = minCount;
            slider.maxValue = maxCount;
        }


        private void Update()
        {
            CurrentValue = (int)slider.value;
            currentCountText.text = prefix + CurrentValue;
        }
    }
}