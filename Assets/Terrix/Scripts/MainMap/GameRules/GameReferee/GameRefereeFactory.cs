using System;
using System.Collections.Generic;
using Terrix.DTO;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public interface IGameRefereeFactory
    {
        GameReferee Create(IEnumerable<Player> players);
    }

    public class GameRefereeFactory : IGameRefereeFactory
    {
        private readonly GameReferee.Settings settings;
        public GameRefereeFactory(GameReferee.Settings settings)
        {
            this.settings = settings;
        }

        public GameReferee Create(IEnumerable<Player> players)
        {
            return settings.Type switch
            {
                GameModeType.FFA => new FFAGameReferee(players),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}