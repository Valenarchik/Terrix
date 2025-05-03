using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Terrix
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private LeaderboardUI leaderboardUI;
        [SerializeField] private BottomGamePanelUI bottomGamePanelUI;
        [SerializeField] private PlayersCountriesSizePieChartUI playersCountriesSizePieChartUI;
        [SerializeField] private GameObject winWindow;
        [SerializeField] private GameObject loseWindow;
        [SerializeField] private Button menuButton;
        [SerializeField] private GameObject defenseInfoWindow;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            leaderboardUI.gameObject.SetActive(false);
            bottomGamePanelUI.gameObject.SetActive(false);
            playersCountriesSizePieChartUI.gameObject.SetActive(false);
            winWindow.gameObject.SetActive(false);
            loseWindow.gameObject.SetActive(false);
            defenseInfoWindow.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void Initialize(IPlayersProvider playersProvider, Player player)
        {
            leaderboardUI.Initialize(playersProvider);
            bottomGamePanelUI.Initialize(player);
            playersCountriesSizePieChartUI.Initialize(playersProvider);
            playersCountriesSizePieChartUI.gameObject.SetActive(true);
            defenseInfoWindow.gameObject.SetActive(true);
        }

        public void UpdateInfo()
        {
            leaderboardUI.UpdateInfo();
            bottomGamePanelUI.UpdateInfo();
            playersCountriesSizePieChartUI.UpdateInfo();
        }
        public void WinGame()
        {
            winWindow.SetActive(true);
            menuButton.gameObject.SetActive(false);
            // exitButton.gameObject.SetActive(true);
        }

        public void LoseGame()
        {
            loseWindow.SetActive(true);
            menuButton.gameObject.SetActive(false);
            // exitButton.gameObject.SetActive(true);
        }
    }
}