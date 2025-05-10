using System;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Entities
{
    public class Player : Entity<int>
    {
        private Country country;
        public PlayerType PlayerType { get; }
        public string PlayerName { get; set; }
        public Color PlayerColor { get; set; }

        public PlayerState PlayerState { get; private set; }

        public bool IsLose => PlayerState == PlayerState.Lose;
        public bool IsWin => PlayerState == PlayerState.Win;
        public DateTime LoseTime { get; private set; }
        // public static event Action<int, bool> OnGameEnd;
        public event Action<int, bool> OnGameEnd;

        [NotNull] public Country Country
        {
            get => country;
            set => country = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Player(int id, PlayerType playerType) : base(id)
        {
            PlayerType = playerType;
            PlayerState = PlayerState.InGame;
        }

        public void Win()
        {
            PlayerState = PlayerState.Win;
            OnGameEnd?.Invoke(ID, true);
        }

        public void Lose()
        {
            Debug.Log("Player lose");
            PlayerState = PlayerState.Lose;

            LoseTime = DateTime.UtcNow;
            Country.Population = 0;
            Country.Clear();
            OnGameEnd?.Invoke(ID, false);
        }

        public Player(int id, PlayerType playerType, Country country, string playerName, Color color) : base(id)
        {
            PlayerType = playerType;
            Country = country;
            Country.Owner = this;
            PlayerName = playerName;
            PlayerColor = color;
        }
    }
}