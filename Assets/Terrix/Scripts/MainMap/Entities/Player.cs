using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    public class Player
    {
        public int ID { get; }
        public PlayerType PlayerType { get; }
        [MaybeNull] public Country Country { get; set; }
        
        public Player(int id, PlayerType playerType)
        {
            ID = id;
            PlayerType = playerType;
        }
        public Player(PlayerType playerType, Country country)
        {
            PlayerType = playerType;
            Country = country;
        }

        public void Lose()
        {
            Country = null;
        }
    }
}