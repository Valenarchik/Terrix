using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public interface IPlayersProvider
    {
        Player[] GetAll();
        Player Find(int id);
    }

    public class PlayersProvider : IPlayersProvider
    {
        private readonly List<Player> players;

        public Player[] GetAll() => players.ToArray();
        public Player Find(int id)
        {
            return players.Find(player => player.ID == id);
        }

        public PlayersProvider(IEnumerable<Player> players)
        {
            this.players = players.ToList();
        }
    }
}