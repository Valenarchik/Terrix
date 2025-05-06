using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.Map;

namespace Terrix.Game.GameRules
{
    public interface IAttackInvoker : ITickHandler
    {
        public event Action<Attack> OnStartAttack;
        public event Action<Attack> OnFinishAttack;
        
        public void AddAttack(Attack attack);
    }

    public class AttackInvoker : IAttackInvoker
    {
        private readonly List<Attack> attacksOrder = new();
        private readonly Dictionary<Guid, Attack> attacksMap = new();
        private readonly Dictionary<Guid, AttackState> attacksStates = new();
        
        public event Action<Attack> OnStartAttack;
        public event Action<Attack> OnFinishAttack;

        public void AddAttack(Attack attack)
        {
            if (attack is null || (!attack.IsGlobalAttack && attack.Territory.Count == 0))
            {
                return;
            }

            if (attacksMap.TryAdd(attack.ID, attack))
            {
                attacksOrder.Add(attack);
                attacksStates.Add(attack.ID, new AttackState(attack, attack.Points));
                attack.Owner.Country.Population -= attack.Points;
                OnStartAttack?.Invoke(attack);
            }
        }

        public void HandleTick()
        {
            Attacks();
        }

        private void Attacks()
        {
            foreach (var attack in attacksOrder.ToArray())
            {
                AttackTick(attacksStates[attack.ID]);
            }
        }

        private void AttackTick(AttackState attackState)
        {
            var intersection = GetIntersection(attackState);

            if (intersection.Count == 0)
            {
                FinishAttack(attackState.Attack);
                return;
            }

            var temp = new HashSet<Hex>();
            float totalCost = 0;
            foreach (var hex in intersection)
            {
                var hexCost = hex.GetCost();
                totalCost += hexCost;
                
                if (totalCost > attackState.CurrentPoints)
                {
                    totalCost -= hexCost;
                    break;
                }

                temp.Add(hex);
            }

            intersection = temp;

            if (intersection.Count == 0)
            {
                FinishAttack(attackState.Attack);
                return;
            }

            attackState.CurrentPoints -= totalCost;

            var attackTarget = attackState.Attack.Target;
            if (attackTarget is not null)
            {
                attackTarget.Country.Population -= intersection.Count * attackTarget.Country.DensePopulation;
                attackTarget.Country.Remove(intersection);
            }
            
            attackState.Attack.Owner.Country.Add(intersection);
        }

        private static HashSet<Hex> GetIntersection(AttackState attackState)
        {
            return attackState.Attack.IsGlobalAttack
                ? attackState.Attack.Owner.Country.GetOuterBorder()
                    .Where(h => h.GetHexData().CanCapture && h.PlayerId == attackState.Attack.Target?.ID)
                    .ToHashSet()
                : attackState.Attack.Territory
                    .Where(h => h.GetHexData().CanCapture && h.PlayerId == attackState.Attack.Target?.ID)
                    .Intersect(attackState.Attack.Owner.Country.GetOuterBorder())
                    .ToHashSet();
        }

        private void FinishAttack(Attack attack)
        {
            var state = attacksStates[attack.ID];
            attack.Owner.Country.Population += state.CurrentPoints;
            
            RemoveAttackInternal(attack);
            
            OnFinishAttack?.Invoke(attack);
        }
        
        private void RemoveAttackInternal(Attack attack)
        {
            if (attacksMap.Remove(attack.ID))
            {
                attacksOrder.Remove(attack);
                attacksStates.Remove(attack.ID);
            }
        }
        
        private class AttackState
        {
            public Attack Attack { get; }
            public float CurrentPoints { get; set; }

            public AttackState(Attack attack, float currentPoints)
            {
                Attack = attack;
                CurrentPoints = currentPoints;
            }
        }
    }
}