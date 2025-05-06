using System.Linq;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using TMPro;
using UnityEngine;

namespace Terrix
{
    public class BottomGamePanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private TextMeshProUGUI populationGainText;
        [SerializeField] private TextMeshProUGUI densePopulationText;
        [SerializeField] private TextMeshProUGUI farmlandHexesCountText;
        [SerializeField] private TextMeshProUGUI grasslandHexesCountText;
        [SerializeField] private TextMeshProUGUI forestHexesCountText;
        [SerializeField] private TextMeshProUGUI hillHexesCountText;
        [SerializeField] private TextMeshProUGUI mountainHexesCountText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        private Player player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Initialize(Player player)
        {
            gameObject.SetActive(true);
            this.player = player;
            SetStartInfo();
        }

        private void SetStartInfo()
        {
            populationGainText.text = $"+0";
            populationGainText.text = $"+0";
            densePopulationText.text = $"0";
            farmlandHexesCountText.text = $"x0";
            grasslandHexesCountText.text = $"x0";
            hillHexesCountText.text = $"x0";
            forestHexesCountText.text = $"x0";
            mountainHexesCountText.text = $"x0";
            playerNameText.text = player.PlayerName;
        }

        public void UpdateInfo()
        {
            populationGainText.text = $"+{(int)player.Country.GetIncome()}";
            densePopulationText.text = $"{(int)player.Country.DensePopulation}";
            populationText.text = ((int)player.Country.Population).ToString();
            var cells = player.Country.Cells.ToArray();

            farmlandHexesCountText.text = $"x{cells.Count(cell => cell.HexType is HexType.Farmlands)}";
            grasslandHexesCountText.text = $"x{cells.Count(cell => cell.HexType is HexType.Grasslands)}";
            hillHexesCountText.text = $"x{cells.Count(cell => cell.HexType is HexType.Hills)}";
            forestHexesCountText.text = $"x{cells.Count(cell => cell.HexType is HexType.Forest)}";
            mountainHexesCountText.text = $"x{cells.Count(cell => cell.HexType is HexType.Mountain)}";
        }
    }
}