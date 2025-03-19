using TMPro;
using UnityEngine;

namespace Terrix.Networking
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playersCountText;
        [SerializeField] private TextMeshProUGUI lobbyStateText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Lobby lobby;

        private void Start()
        {
            if (lobby.IsServerInitialized)
            {
                playersCountText.gameObject.SetActive(false);
                lobbyStateText.gameObject.SetActive(false);
                timerText.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            lobby.OnPlayersChanged += LobbyOnPlayersChanged;
            lobby.OnStateChanged += LobbyOnStateChanged;
            lobby.OnTimerChanged += LobbyOnTimerChanged;
        }

        private void LobbyOnTimerChanged(float time)
        {
            timerText.text = $"Время до следущего этапа: {((int)time).ToString()}с.";
        }

        private void LobbyOnStateChanged(string lobbyStateName)
        {
            lobbyStateText.text = lobbyStateName;
        }

        private void LobbyOnPlayersChanged()
        {
            playersCountText.text =
                $"Число игроков в комнате: {lobby.PlayersCurrentCount.ToString()}/ {lobby.PlayersMaxCount.ToString()}";
        }
    }
}