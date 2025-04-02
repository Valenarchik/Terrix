namespace Terrix.Game.GameRules
{
    /// <summary>
    /// Все против всех
    /// </summary>
    public class FFAGameReferee: GameReferee
    {
        public FFAGameReferee(IPlayersProvider playersProvider) : base(playersProvider)
        {
        }

        public override void HandleTick()
        {
            // TODO: Проверка, что игроки имеют достаточно территорий;
        }
    }
}