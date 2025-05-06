using System.Collections.Generic;
using System.Linq;
using Terrix.Game.GameRules;
using UnityEngine;

namespace Terrix.Networking
{
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private MainMapEntryPoint mainMapEntryPoint;
        [SerializeField] private PlayerInfoUI playerInfoUIPrefab;
        [SerializeField] private PlayerInfoUI titles;
        [SerializeField] private RectTransform listRoot;
        private Dictionary<int, PlayerInfoUI> playersInfos = new();
        private PlayerInfoUI[] currentPlayersInfos;
        private IPlayersProvider playersProvider;
        private int defaultLeaderboardLength = 10;
        private int currentLeaderboardLength;
        private float horizontalBgiCompressedWidth = 530;
        private float horizontalBgiExpandedWidth = 820;
        private float verticalOffset = 40;
        private RectTransform rectTransform;
        private bool needSetRectTransform;

        private void Update()
        {
            if (needSetRectTransform)
            {
                rectTransform.sizeDelta =
                    new Vector2(horizontalBgiCompressedWidth, listRoot.sizeDelta.y + verticalOffset);
                needSetRectTransform = false;
                Compress();
            }
        }

        public void Initialize(IPlayersProvider playersProvider)
        {
            rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(true);
            this.playersProvider = playersProvider;
            var players = this.playersProvider.GetAll().ToArray();
            var index = 1;
            currentLeaderboardLength = Mathf.Min(defaultLeaderboardLength, players.Length);
            var nonWaterHexesCount = mainMapEntryPoint.Map.GetNonWaterHexesCount();
            currentPlayersInfos = new PlayerInfoUI[currentLeaderboardLength];
            foreach (var player in players)
            {
                var playerInfoUI = Instantiate(playerInfoUIPrefab, listRoot);
                playerInfoUI.Initialize(player, index, nonWaterHexesCount);
                if (index > defaultLeaderboardLength)
                {
                    playerInfoUI.gameObject.SetActive(false);
                }

                // playerInfoUI.Initialize(player.PlayerName, player.PlayerColor);
                playersInfos.Add(player.ID, playerInfoUI);
                if (index <= currentLeaderboardLength)
                {
                    currentPlayersInfos[index - 1] = playerInfoUI;
                }

                index++;
            }

            needSetRectTransform = true;
            // rectTransform.sizeDelta = new Vector2(horizontalBgiCompressedWidth, listRoot.sizeDelta.y + verticalOffset);
        }

        public void UpdateInfo()
        {
            // var players = playersProvider.GetAll().ToArray();
            var sortedPlayersInfos = playersInfos.OrderByDescending(pair => playersProvider
                .Find(pair.Key).Country.Population).ToArray();
            for (int i = 0; i < currentLeaderboardLength; i++)
            {
                sortedPlayersInfos[i].Value.RectTransform.SetSiblingIndex(i + 1);
                sortedPlayersInfos[i].Value.UpdateInfo(i + 1);
                sortedPlayersInfos[i].Value.gameObject.SetActive(true);
                currentPlayersInfos[i] = sortedPlayersInfos[i].Value;
            }

            for (int i = currentLeaderboardLength; i < sortedPlayersInfos.Length; i++)
            {
                sortedPlayersInfos[i].Value.gameObject.SetActive(false);
            }
        }

        public void Expand()
        {
            titles.Expand();
            rectTransform.sizeDelta = new Vector2(horizontalBgiExpandedWidth, rectTransform.sizeDelta.y);
            foreach (var playerInfoUI in playersInfos.Values)
            {
                playerInfoUI.Expand();
            }
            // for (var i = 0; i < currentLeaderboardLength; i++)
            // {
            //     currentPlayersInfos[i].Expand();
            // }
        }

        public void Compress()
        {
            titles.Compress();
            rectTransform.sizeDelta = new Vector2(horizontalBgiCompressedWidth, rectTransform.sizeDelta.y);
            foreach (var playerInfoUI in playersInfos.Values)
            {
                playerInfoUI.Compress();
            }
            // for (var i = 0; i < currentLeaderboardLength; i++)
            // {
            //     currentPlayersInfos[i].Compress();
            // }
        }
    }
}