using System.Collections.Generic;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public interface IGameReferee: ITickHandler
    {
    }

    public abstract class GameReferee: IGameReferee
    {
        protected readonly List<Player> players;

        protected GameReferee(IEnumerable<Player> players)
        {
            this.players = players.Where(player => player is not null).ToList();
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