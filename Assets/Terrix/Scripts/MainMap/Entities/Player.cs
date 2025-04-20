using System;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    public class Player: Entity<int>
    {
        private Country country;
        public PlayerType PlayerType { get; }
        public PlayerState PlayerState { get; private set; }
        
        public bool IsLose => PlayerState == PlayerState.Lose;
        public bool IsWin => PlayerState == PlayerState.Win;
        public DateTime LoseTime { get; private set; }

        [NotNull] public Country Country
        {
            get => country;
            set => country = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Player(int id, PlayerType playerType): base(id)
        {
            PlayerType = playerType;
            PlayerState = PlayerState.InGame;
        }

        public void Win()
        {
            PlayerState = PlayerState.Win;
        }
        
        public void Lose()
        {
            PlayerState = PlayerState.Lose;
            
            LoseTime = DateTime.UtcNow;
            Country.Population = 0;
            Country.Clear();
        }
    }
}