using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    public class Player : Entity<int>
    {
        public PlayerType PlayerType { get; }
        [MaybeNull] public Country Country { get; set; }
        public string PlayerName { get; set; }

        public Player(int id, PlayerType playerType) : base(id)
        {
            PlayerType = playerType;
        }

        public Player(int id, PlayerType playerType, Country country, string playerName) : base(id)
        {
            PlayerType = playerType;
            Country = country;
            Country.Owner = this;
            PlayerName = playerName;
        }

        public void Lose()
        {
            Country = null;
        }
    }
}