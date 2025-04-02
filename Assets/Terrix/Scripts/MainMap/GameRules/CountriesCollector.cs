namespace Terrix.Game.GameRules
{
    public interface ICountriesCollector : ITickHandler
    {
    }

    public class CountriesCollector: ICountriesCollector
    {
        private readonly IPlayersProvider playersProvider;

        public CountriesCollector(IPlayersProvider playersProvider)
        {
            this.playersProvider = playersProvider;
        }
        
        public void HandleTick()
        {
            foreach (var player in playersProvider.GetAll())
            {
                player.Country?.CollectIncome();;
            }
        }
    }
}