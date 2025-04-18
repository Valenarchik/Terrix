using Terrix.DTO;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Entities
{
    public class Player : Entity<int>
    {
        public PlayerType PlayerType { get; }
        public Country Country { get; set; }
        public string PlayerName { get; set; }
        public Color PlayerColor { get; set; }

        public Player(int id, PlayerType playerType) : base(id)
        {
            PlayerType = playerType;
        }

        public Player(int id, PlayerType playerType, Country country, string playerName, Color color) : base(id)
        {
            PlayerType = playerType;
            Country = country;
            Country.Owner = this;
            PlayerName = playerName;
            PlayerColor = color;
        }

        public void Lose()
        {
            Country = null;
        }
    }
}