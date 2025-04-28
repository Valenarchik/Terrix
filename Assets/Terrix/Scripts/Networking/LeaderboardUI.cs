using System.Collections.Generic;
using System.Linq;
using Terrix.Game.GameRules;
using UnityEngine;

namespace Terrix.Networking
{
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private PlayerInfoUI playerInfoUIPrefab;
        private Dictionary<int, PlayerInfoUI> playersInfos = new();
        private IPlayersProvider playersProvider;

        public void Initialize(IPlayersProvider playersProvider)
        {
            this.playersProvider = playersProvider;
            var index = 1;
            foreach (var player in playersProvider.GetAll())
            {
                var playerInfoUI = Instantiate(playerInfoUIPrefab, transform);
                playerInfoUI.Initialize(player, index);
                if (index > 10)
                {
                    playerInfoUI.gameObject.SetActive(false);
                }

                // playerInfoUI.Initialize(player.PlayerName, player.PlayerColor);
                playersInfos.Add(player.ID, playerInfoUI);
                index++;
            }

            for (int i = 0; i < playersInfos.Count; i++)
            {
                
            }
        }

        public void UpdateInfo()
        {
            // var players = playersProvider.GetAll().ToArray();
            var defaultLeaderboardLength = 10;
            var sortedPlayersInfos = playersInfos.OrderByDescending(pair => playersProvider
                .Find(pair.Key).Country.Population).ToArray();
            var currentLeaderboardLength = Mathf.Min(defaultLeaderboardLength, sortedPlayersInfos.Length);
            for (int i = 0; i < currentLeaderboardLength; i++)
            {
                sortedPlayersInfos[i].Value.RectTransform.SetSiblingIndex(i);
                sortedPlayersInfos[i].Value.UpdateInfo(i + 1);
                sortedPlayersInfos[i].Value.gameObject.SetActive(true);

            }

            for (int i = currentLeaderboardLength; i < sortedPlayersInfos.Length; i++)
            {
                sortedPlayersInfos[i].Value.gameObject.SetActive(false);
            }

        }
    }
}