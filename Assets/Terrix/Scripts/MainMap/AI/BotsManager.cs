using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Extensions;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Settings;
using Random = UnityEngine.Random;

namespace Terrix.MainMap.AI
{
    public interface IBotsManager: ITickHandler
    {
        public void AddBots(IEnumerable<Bot> botsEnumerable);
    }

    public class BotsManager: IBotsManager
    {
        private readonly Dictionary<int, BotState> bots = new();

        private readonly IGameDataProvider gameDataProvider;
        private readonly IPlayersProvider playersProvider;
        private readonly IAttackInvoker attackInvoker;

        public BotsManager(IAttackInvoker attackInvoker, IGameDataProvider gameDataProvider, IPlayersProvider playersProvider)
        {
            this.attackInvoker = attackInvoker;
            this.gameDataProvider = gameDataProvider;
            this.playersProvider = playersProvider;

            attackInvoker.OnStartAttack += AttackInvokerOnStartAttack;
            attackInvoker.OnFinishAttack += AttackInvokerOnFinishAttack;
        }

        private void AttackInvokerOnStartAttack(Attack attack)
        {
            if (attack.Target == null)
            {
                return;
            }

            var botId = attack.Target.ID;
            
            if (bots.ContainsKey(botId) && attack.Target.PlayerType == PlayerType.Player)
            {
                bots[botId].PriorityTarget = attack.Owner;
            }
        }
        
        private void AttackInvokerOnFinishAttack(Attack attack)
        {
            if (bots.TryGetValue(attack.Owner.ID, out BotState botState))
            {
                botState.CurrentAttack = null;
            }
        }

        public void AddBots(IEnumerable<Bot> botsEnumerable)
        {
            foreach (var bot in botsEnumerable.Where(b => b != null))
            {
                if (!bots.ContainsKey(bot.ID))
                {
                    bots.Add(bot.ID, new BotState
                    {
                        Bot = bot,
                        Settings = gameDataProvider.Get().BotSettings ?? throw new NullReferenceException(),
                        NextLaunch = DateTime.UtcNow
                    });
                }
            }
        }
        
        public void HandleTick()
        {
            foreach (var (id, botState) in bots)
            {
                if (botState.Bot.PlayerState == PlayerState.InGame 
                    && botState.NextLaunch <= DateTime.UtcNow)
                {
                    LaunchBot(botState);
                    botState.NextLaunch = CalculateNextLaunchTime(botState);
                }
            }
        }

        private void LaunchBot(BotState botState)
        {
            if (botState.CurrentAttack != null)
            {
                return;
            }
            
            var gameSettings = gameDataProvider.Get();
            var botSettings = botState.Settings;
            var botCountry = botState.Bot.Country;
            var percentOfPopulation = botCountry.DensePopulation / gameSettings.MaxDensePopulation;

            if (percentOfPopulation < botSettings.minPercentOfPopulation)
            {
                return;
            }

            if (botState.PriorityTarget != null)
            {
                if (botState.PriorityTarget.PlayerState == PlayerState.InGame)
                {
                    StartAttack(botState, botState.PriorityTarget);
                    return;
                }

                botState.PriorityTarget = null;
                return;
            }

            var neighbours = FindNeighbours(botState.Bot);
            
            Player target;
                
            if (neighbours.Any(p => p.PlayerType == PlayerType.Player) 
                && Random.Range(0f, 1f) < botState.Settings.chanceToAttackPlayer)
            {
                target = neighbours.Where(p => p.PlayerType == PlayerType.Player).RandomElement();
            }
            else if (neighbours.Any(p => p.PlayerType == PlayerType.Bot)
                     && Random.Range(0f, 1f) < botState.Settings.chanceToAttackBot)
            {
                target = neighbours.Where(p => p.PlayerType == PlayerType.Bot).RandomElement();
            }
            else
            {
                target = null;
            }
            
            botState.PriorityTarget = target;
            StartAttack(botState, target);
        }

        private Player[] FindNeighbours(Bot bot)
        {
            return bot.Country.GetOuterBorder()
                .Select(hex => hex.PlayerId)
                .Distinct()
                .Where(playerId => playerId.HasValue)
                .Select(playerId => playersProvider.Find(playerId.Value))
                .ToArray();
        }
        
        private void StartAttack(BotState botState, Player target = null)
        {
            var botSettings = botState.Settings;
            var botCountry = botState.Bot.Country;
            var percentOfAttackCost = Random.Range(botSettings.minAttackCost,botSettings.maxAttackCost);
            var attackPoints = botCountry.Population * percentOfAttackCost;
            var attack = new AttackBuilder
            {
                Owner = botState.Bot,
                Points = attackPoints,
                Target = target,
                IsGlobalAttack = true
            }.Build();
            
            attackInvoker.AddAttack(attack);
        }
        
        private DateTime CalculateNextLaunchTime(BotState botState)
        {
            var timeout = TimeSpan.FromSeconds(botState.Settings.timeoutInSeconds);
            var deflection = botState.Settings.timeoutDeflection * Random.Range(0f, 1f);
            return DateTime.UtcNow + timeout * deflection;
        }
        
        private class BotState
        {
            public Bot Bot { get; set; }
            public BotSettings Settings { get; set; }
            public DateTime NextLaunch {get; set;}
            
            public Attack CurrentAttack { get; set; }
            public Player PriorityTarget { get; set; }
        }
    }
}