using System.Collections.Generic;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    /// <summary>
    /// Все против всех
    /// </summary>
    public class FFAGameReferee: GameReferee
    {
        public FFAGameReferee(IEnumerable<Player> players) : base(players)
        {
        }

        public override void HandleTick()
        {
            // TODO: Проверка, что игроки имеют достаточно территорий;
        }
    }
}