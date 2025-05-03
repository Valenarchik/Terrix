using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terrix.Entities;
using Terrix.Game.GameRules;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayersCountriesSizePieChartUI : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image circleImagePrefab;
    [SerializeField] private RectTransform ringImage;
    private Dictionary<Player, Image> playersImages = new();

    public float TotalPercentage()
    {
        float total = 0;
        foreach (Player player in playersImages.Keys)
        {
            total += player.Country.TotalCellsCount;
        }

        return total;
    }

    public void Initialize(IPlayersProvider players)
    {
        gameObject.SetActive(true);
        foreach (var player in players.GetAll())
        {
            // var piePiece = new PiePiece(player.PlayerColor, player.Country.TotalCellsCount);
            var image = Instantiate(circleImagePrefab, root);
            // image.rectTransform.sizeDelta = ringImage.sizeDelta;
            playersImages.Add(player, image);
            image.color = player.PlayerColor;
        }

        UpdateInfo();
    }

    public void UpdateInfo()
    {
        var percentage = TotalPercentage();
        var currentRotation = 0f;
        foreach (var player in playersImages.Keys)
        {
            var image = playersImages[player];
            float percent;
            if (percentage == 0)
            {
                percent = 1 / playersImages.Count;
            }
            else
            {
                percent = player.Country.TotalCellsCount / percentage;
            }

            image.fillAmount = percent;
            image.rectTransform.rotation = Quaternion.Euler(0, 0, currentRotation);
            currentRotation += percent * -360;
        }
    }
}