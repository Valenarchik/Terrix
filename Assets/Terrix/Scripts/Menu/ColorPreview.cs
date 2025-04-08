using System;
using UnityEngine;
using UnityEngine.UI;

namespace Terrix.Menu
{
    public class ColorPreview : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Image secondImage;
        [SerializeField] private Slider redColorSlider;
        [SerializeField] private Slider greenColorSlider;
        [SerializeField] private Slider blueColorSlider;

        private void Update()
        {
            var newColor = image.color;
            newColor.r = redColorSlider.value;
            newColor.g = greenColorSlider.value;
            newColor.b = blueColorSlider.value;
            image.color = newColor;
            secondImage.color = newColor;
        }
    }
}