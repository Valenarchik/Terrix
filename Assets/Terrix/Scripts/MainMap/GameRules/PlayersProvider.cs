using System.Collections.Generic;
using System.Linq;
using Terrix.Entities;

namespace Terrix.Game.GameRules
{
    public interface IPlayersProvider
    {
        Player[] GetAll();
        Player Find(int id);
        Player[] Find(IEnumerable<int> ids);
    }

    public class PlayersProvider : IPlayersProvider
    {
        private readonly List<Player> players;
        public List<Player> Players => players;
        private Dictionary<int, Player> playersMap;

        public Player[] GetAll() => players.ToArray();

        public Player Find(int id)
        {
            playersMap.TryGetValue(id, out var p);
            return p;
        }

        public Player[] Find(IEnumerable<int> ids)
        {
            return ids.Where(id => playersMap.ContainsKey(id)).Select(id => playersMap[id]).ToArray();
        }

        public PlayersProvider(IEnumerable<Player> players)
        {
            this.players = players.ToList();
            this.playersMap = this.players.ToDictionary(p => p.ID);
        }

        public void UpdatePlayersMap()
        {
            playersMap = players.ToDictionary(p => p.ID);
        }
    }
}