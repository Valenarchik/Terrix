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

        public GameRefereeFactory(GameReferee.Settings settings, IPlayersProvider playersProvider)
        {
            this.settings = settings;
            this.playersProvider = playersProvider;
        }

        public GameReferee Create()
        {
            return settings.Type switch
            {
                GameModeType.FFA => new FFAGameReferee(playersProvider),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}