namespace Terrix.Networking
{
    public struct LobbySettings
    {
        public int PlayersCount { get; private set; }
        public int BotsCount { get; private set; }
        
        public LobbySettings(int playersCount, int botsCount)
        {
            PlayersCount = playersCount;
            BotsCount = botsCount;
        }
    }
}