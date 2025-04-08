using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Terrix.Entities;
using Terrix.Game.GameRules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terrix.Networking
{
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private PlayerInfoUI playerInfoUIPrefab;
        private Dictionary<int, PlayerInfoUI> playersInfos = new();

        public void Initialize(IPlayersProvider playersProvider)
        {
            foreach (var player in playersProvider.GetAll())
            {
                var playerInfoUI = Instantiate(playerInfoUIPrefab, transform);
                playerInfoUI.Initialize(player.PlayerName, player.PlayerColor);
                playersInfos.Add(player.ID, playerInfoUI);
            }
        }

        public void UpdateInfo(IPlayersProvider playersProvider)
        {
            var players = playersProvider.GetAll();
            foreach (var pair in playersInfos
                         .OrderByDescending(pair => playersProvider.Find(pair.Key).Country.Population))
            {
                pair.Value.RectTransform.SetAsLastSibling();
            }

            for (var i = 0; i < players.Length; i++)
            {
                playersInfos[i].UpdateInfo(players[i].Country.Population);
            }
        }
    }
}