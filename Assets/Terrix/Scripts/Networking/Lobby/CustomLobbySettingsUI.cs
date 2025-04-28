using UnityEngine;

namespace Terrix.Networking
{
    public class CustomLobbySettingsUI : MonoBehaviour
    {
        [SerializeField] private PlayersCountSettingsUI playersCountSettings;
        [SerializeField] private PlayersCountSettingsUI botsCountSettings;

        public LobbySettings GetLobbySettings()
        {
            return new LobbySettings(playersCountSettings.CurrentValue, botsCountSettings.CurrentValue);
        }
    }
}