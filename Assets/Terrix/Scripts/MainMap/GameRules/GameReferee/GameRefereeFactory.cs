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
        private readonly IGame events;

        public GameRefereeFactory(
            GameReferee.Settings settings,
            IPlayersProvider playersProvider,
            IGame events) 
        {
            this.settings = settings;
            this.playersProvider = playersProvider;
            this.events = events;
        }

        public GameReferee Create()
        {
            return settings.Type switch
            {
                GameModeType.FFA => new FFAGameReferee(settings, playersProvider, events),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}