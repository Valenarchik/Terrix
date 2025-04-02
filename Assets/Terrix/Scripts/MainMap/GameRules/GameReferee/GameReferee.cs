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
        protected IPlayersProvider playersProvider;
        
        protected GameReferee(IPlayersProvider playersProvider)
        {
            this.playersProvider = playersProvider;
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