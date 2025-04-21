using System.Collections.Generic;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public interface IGameReferee : ITickHandler
    {
    }

    public abstract class GameReferee : IGameReferee
    {
        protected readonly Settings settings;
        protected readonly IPlayersProvider playersProvider;
        protected readonly IGame game;

        protected GameReferee(Settings settings, IPlayersProvider playersProvider, IGame game)
        {
            this.settings = settings;
            this.playersProvider = playersProvider;
            this.game = game;
        }

        public void HandleTick()
        {
            HandleTickInternal();
        }

        protected abstract void HandleTickInternal();
        
        protected IEnumerable<Player> FindNewLosePlayers()
        {
            foreach (var player in playersProvider.GetAll().Where(p => !p.IsLose))
            {
                float losePercent;

                if (player.Country.MaxCellsCount == 0)
                {
                    losePercent = 1;
                }
                else
                {
                    losePercent = (float) player.Country.TotalCellsCount / player.Country.MaxCellsCount;
                }

                if (losePercent < settings.PercentOfLose)
                {
                    yield return player;
                }
            }
        }

        public class Settings
        {
            public GameModeType Type { get; }
            public float PercentOfLose { get; set; }

            public Settings(GameModeType type, float percentOfLose)
            {
                Type = type;
                PercentOfLose = percentOfLose;
            }
        }
    }
}