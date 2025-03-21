using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    public class Player
    {
        public PlayerType PlayerType { get; }
        [MaybeNull] public Country Country { get; set; }

        public Player(PlayerType playerType)
        {
            PlayerType = playerType;
        }

        public Player(PlayerType playerType, Country country)
        {
            PlayerType = playerType;
            Country = country;
        }
    }
}