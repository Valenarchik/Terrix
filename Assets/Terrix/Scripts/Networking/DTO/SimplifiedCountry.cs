namespace Terrix.Network.DTO
{
    public class SimplifiedCountry
    {
        public int PlayerId { get; private set; }
        public float Population { get; private set; }
        public int CellsCount { get; private set; }

        public SimplifiedCountry(int playerId, float population, int cellsCount)
        {
            PlayerId = playerId;
            Population = population;
            CellsCount = cellsCount;
        }
    }
}