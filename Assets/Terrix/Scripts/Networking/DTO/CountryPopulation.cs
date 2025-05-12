namespace Terrix.Network.DTO
{
    public class CountryPopulation
    {
        public int PlayerId { get; private set; }
        public float Population { get; private set; }

        public CountryPopulation(int playerId, float population)
        {
            PlayerId = playerId;
            Population = population;
        }
    }
}