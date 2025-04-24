using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Terrix.Networking
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playersCountText;
        [SerializeField] private TextMeshProUGUI lobbyStateAndTimeText;
        // [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI idText;
        [SerializeField] private Lobby lobby;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        private string stateString;

        private void Start()
        {
            if (lobby.IsServerInitialized)
            {
                playersCountText.gameObject.SetActive(false);
                lobbyStateAndTimeText.gameObject.SetActive(false);
                // timerText.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            lobby.OnPlayersChanged += LobbyOnPlayersChanged;
            lobby.OnStateChanged += LobbyOnStateChanged;
            lobby.OnTimerChanged += LobbyOnTimerChanged;
            lobby.OnStartInfoSet += SetLobbyId;
        }

        public void SetLobbyId()
        {
            idText.text = $"Lobby id: {lobby.Id}";
        }

        private void LobbyOnTimerChanged(float time)
        {
            if (time > 0)
            {
                lobbyStateAndTimeText.text = $"{stateString} {(int)time}с.";
            }
            else
            {
                lobbyStateAndTimeText.text = stateString;
            }
        }

        private void LobbyOnStateChanged(LobbyStateType lobbyStateType)
        {
            switch (lobbyStateType)
            {
                case LobbyStateType.Searching:
                    stateString = "До начала игры осталось ";
                    break;
                case LobbyStateType.BeforeStarting:
                    exitButton.gameObject.SetActive(false);
                    playersCountText.gameObject.SetActive(false);
                    stateString = "Игра начинается через ";
                    break;
                case LobbyStateType.Starting:
                    stateString = "Выбор территории закончится через ";
                    break;
                case LobbyStateType.Playing:
                    lobbyStateAndTimeText.gameObject.SetActive(false);
                    stateString = "Игра идёт";
                    break;
                case LobbyStateType.Ended:
                    stateString = "Игра окончена";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lobbyStateType), lobbyStateType, null);
            }

            lobbyStateAndTimeText.text = stateString;
        }

        private void LobbyOnPlayersChanged()
        {
            playersCountText.text =
                $"Число игроков в комнате: {lobby.PlayersCurrentCount.ToString()}/ {lobby.PlayersMaxCount.ToString()}";
        }

        public void WinGame()
        {
            winPanel.SetActive(true);
            exitButton.gameObject.SetActive(true);
        }

        public void LoseGame()
        {
            losePanel.SetActive(true);
            exitButton.gameObject.SetActive(true);
        }
    }
}