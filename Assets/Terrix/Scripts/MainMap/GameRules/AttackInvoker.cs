using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.Map;

namespace Terrix.Game.GameRules
{
    public interface IAttackInvoker : ITickHandler
    {
        public void AddAttack(Attack attack);
        public void RemoveAttack(Attack attack);
    }

    public class AttackInvoker : IAttackInvoker
    {
        private readonly List<Attack> attacksOrder = new();
        private readonly Dictionary<Guid, Attack> attacksMap = new();
        private readonly Dictionary<Guid, AttackState> attacksStates = new();

        public void AddAttack(Attack attack)
        {
            if (attack is null || attack.Territory.Count == 0)
            {
                return;
            }

            if (attacksMap.TryAdd(attack.ID, attack))
            {
                attacksOrder.Add(attack);
                attacksStates.Add(attack.ID, new AttackState(attack, attack.Points));
                attack.Owner.Country.Population += attack.Points;
            }
        }

        public void RemoveAttack(Attack attack)
        {
            RemoveAttackInternal(attack);
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

        private void RemoveAttackInternal(Attack attack)
        {
            if (attacksMap.Remove(attack.ID))
            {
                attacksOrder.Remove(attack);
                attacksStates.Remove(attack.ID);
            }
        }

        private void AttackTick(AttackState attackState)
        {
            // пересечение зоны атаки и внешней обводки
            var intersection = attackState.Attack.Territory
                .Where(h => h.GetHexData().CanCapture && h.PlayerId == attackState.Attack.Target?.ID)
                .Intersect(attackState.Attack.Owner.Country.GetOuterBorder())
                .ToHashSet();

            if (intersection.Count == 0)
            {
                FinishAttack(attackState.Attack);
                return;
            }
            
            var totalCost = intersection.Sum(h => h.GetCost());

            if (totalCost > attackState.CurrentPoints)
            {
                FinishAttack(attackState.Attack);
                return;
            }

            attackState.CurrentPoints -= totalCost;

            if (attackState.Attack.Target is not null)
            {
                attackState.Attack.Target.Country.Remove(intersection);
            }
            
            attackState.Attack.Owner.Country.Add(intersection);
        }

        private void FinishAttack(Attack attack)
        {
            var state = attacksStates[attack.ID];
            attack.Owner.Country.Population += state.CurrentPoints;
            
            RemoveAttackInternal(attack);
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