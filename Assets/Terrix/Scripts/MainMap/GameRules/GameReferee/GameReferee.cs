using System.Collections.Generic;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public abstract class GameReferee: ITickHandler
    {
        protected List<Player> Players;

        protected GameReferee(IEnumerable<Player> players)
        {
            Players = players.Where(player => player is not null).ToList();
        }
        
        public abstract void HandleTick();
        
        
        public class Settings
        {
            public Settings(GameModeType type)
            {
                Type = type;
            }

            public GameModeType Type { get; }
        }
    }
}