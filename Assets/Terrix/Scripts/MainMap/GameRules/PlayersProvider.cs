using System.Collections.Generic;
using System.Linq;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public interface IPlayersProvider
    {
        IEnumerable<Player> GetAll();
        Player Find(int id);
        IEnumerable<Player> Find(IEnumerable<int> ids);
        void AddPlayer(Player player);
        int GetPlayersCount();
    }

    public class PlayersProvider : IPlayersProvider
    {
        private readonly List<Player> players;
        private readonly Dictionary<int, Player> playersMap;

        public IEnumerable<Player> GetAll() => players;

        public Player Find(int id)
        {
            playersMap.TryGetValue(id, out var p);
            return p;
        }

        public IEnumerable<Player> Find(IEnumerable<int> ids)
        {
            return ids.Where(id => playersMap.ContainsKey(id)).Select(id => playersMap[id]);
        }

        public void AddPlayer(Player player)
        {
            if (playersMap.TryAdd(player.ID, player))
            {
                players.Add(player);
            }
        }

        public int GetPlayersCount() => players.Count;

        public PlayersProvider(IEnumerable<Player> players)
        {
            this.players = players.ToList();
            this.playersMap = this.players.ToDictionary(p => p.ID);
        }
    }
}