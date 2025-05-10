using System;

namespace Terrix.DTO
{
    public struct PlayersAndBots
    {
        public int Players { get; }
        public int Bots { get; }
        public int Total { get; }
        
        public PlayersAndBots(int players, int bots)
        {
            // if (players <= 0)
            // {
                // throw new ArgumentOutOfRangeException(nameof(players));
            // }

            if (bots < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bots));
            }

            Players = players;
            Bots = bots;
            Total = players + bots;
        }
    }
}