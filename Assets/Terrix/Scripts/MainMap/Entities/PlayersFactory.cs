using System;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Map;
using Terrix.Settings;

namespace Terrix.Entities
{
    public interface IPlayersFactory
    {
        public Player[] CreatePlayers(PlayersAndBots playersCount);
    }
    
    public class PlayersFactory: IPlayersFactory
    {
        private readonly IGameDataProvider gameDataProvider;
        private readonly HexMap map;

        public PlayersFactory([NotNull] IGameDataProvider gameDataProvider, [NotNull] HexMap map)
        {
            this.gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public Player[] CreatePlayers(PlayersAndBots playersCount)
        {
            var players = new Player[playersCount.Total];

            for (var i = 0; i < playersCount.Total; i++)
            {
                if (i < playersCount.Players)
                {
                    players[i] = new Player(i, PlayerType.Player);
                }
                else
                {
                    players[i] = new Bot(i, PlayerType.Bot);
                }
                players[i].Country = new Country(gameDataProvider, players[i], map);
            }

            return players;
        }
    }
}