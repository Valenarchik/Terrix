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
                // playerInfoUI.Initialize(player.PlayerName, player.PlayerColor);
                playersInfos.Add(player.ID, playerInfoUI);
                index++;
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
            }

            for (int i = currentLeaderboardLength; i < sortedPlayersInfos.Length; i++)
            {
                sortedPlayersInfos[i].Value.gameObject.SetActive(false);
            }

            // foreach (var pair in playersInfos.OrderByDescending(pair =>
            //              playersProvider.Find(pair.Key).Country.Population))
            // {
            //     pair.Value.RectTransform.SetAsLastSibling();
            //     pair.Value.UpdateInfo();
            // }
            //
            // //
            // for (var i = 0; i < players.Length; i++)
            // {
            //     playersInfos[i].UpdateInfo();
            // }
        }
    }
}