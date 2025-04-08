namespace Terrix.DTO
{
    public class GameHexData
    {
        public HexType HexType { get; }
        public float Income { get; }
        public float Resist { get; }
        public bool CanCapture { get; }
        public bool IsSeeTile { get; }

        public GameHexData(
            HexType hexType,
            float income,
            float resist,
            bool canCapture,
            bool isSeeTile)
        {
            HexType = hexType;
            Income = income;
            Resist = resist;
            CanCapture = canCapture;
            IsSeeTile = isSeeTile;
        }
    }
}