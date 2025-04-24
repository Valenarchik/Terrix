using System;
using Terrix.DTO;

namespace Terrix.Game.GameRules
{
    public interface IGameRefereeFactory
    {
        GameReferee Create();
    }

    public class GameRefereeFactory : IGameRefereeFactory
    {
        private readonly GameReferee.Settings settings;
        private readonly IPlayersProvider playersProvider;
        private readonly IGame game;

        public GameRefereeFactory(
            GameReferee.Settings settings,
            IPlayersProvider playersProvider,
            IGame game) 
        {
            this.settings = settings;
            this.playersProvider = playersProvider;
            this.game = game;
        }

        public GameReferee Create()
        {
            return settings.Type switch
            {
                GameModeType.FFA => new FFAGameReferee(settings, playersProvider, game),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}