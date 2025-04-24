using System.Linq;
using MoreLinq;
using Terrix.Entities;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    /// <summary>
    /// Все против всех
    /// </summary>
    public class FFAGameReferee : GameReferee
    {
        public FFAGameReferee(Settings settings, IPlayersProvider playersProvider, IGame game) : base(settings,
            playersProvider, game)
        {
        }

        protected override void HandleTickInternal()
        {
            var newLosePlayers = FindNewLosePlayers().ToArray();

            if (!newLosePlayers.Any())
            {
                return;
            }

            var players = playersProvider.GetAll().ToArray();
            var losePlayersCount = playersProvider.GetAll().Count(p => p.IsLose);
            var totalLosePlayersCount = losePlayersCount + newLosePlayers.Length;
            Debug.Log($"old: {losePlayersCount}; new: {newLosePlayers.Length}; total: {totalLosePlayersCount};");
            Player winner;
            if (totalLosePlayersCount == players.Length)
            {
                winner = newLosePlayers.Last();
            }
            else if (players.Length - totalLosePlayersCount == 1)
            {
                winner = players.Where(p => !p.IsLose).Except(newLosePlayers).Single();
                Debug.Log(winner);
            }
            else
            {
                winner = null;
            }

            newLosePlayers.Where(p => !Equals(p, winner)).ForEach(p => p.Lose());

            if (winner != null)
            {
                winner.Win();
                game.FinishGame();
            }
        }
    }
}