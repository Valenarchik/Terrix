using System;
using System.Linq;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Game.GameRules;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Network.DTO
{
    public class AttackMessage : Entity<Guid>
    {
        public int Owner { get; }
        public int? Target { get; }
        public float Points { get; }
        public bool IsGlobalAttack { get; set; }
        public Vector3Int[] Territory { get; }

        public AttackMessage(Guid id, int owner, int? target, float points, Vector3Int[] territory, bool isGlobalAttack) : base(id)
        {
            Owner = owner;
            Target = target;
            Points = points;
            Territory = territory;
            IsGlobalAttack = isGlobalAttack;
        }
    }

    public interface IAttackMassageEncoder
    {
        AttackMessage Encode([NotNull] Attack attack);
        Attack Decode([NotNull] AttackMessage attackMessage);
    }

    public class AttackMassageEncoder : IAttackMassageEncoder
    {
        private readonly IPlayersProvider players;
        private readonly HexMap map;
        
        public AttackMassageEncoder(IPlayersProvider players, HexMap map)
        {
            this.players = players;
            this.map = map;
        }


        public AttackMessage Encode([NotNull] Attack attack)
        {
            if (attack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            return new AttackMessage(attack.ID,
                attack.Owner.ID,
                attack.Target?.ID,
                attack.Points,
                attack.Territory?.Where(h => h.PlayerId == attack.Target?.ID).Select(h => h.Position).ToArray(),
                attack.IsGlobalAttack);
        }

        public Attack Decode([NotNull] AttackMessage attackMessage)
        {
            if (attackMessage == null)
            {
                throw new ArgumentNullException(nameof(attackMessage));
            }

            var owner = players.Find(attackMessage.Owner);
            var target = attackMessage.Target.HasValue ? players.Find(attackMessage.Target.Value) : null;
            var territory = attackMessage.Territory?.Where(v => map.HasHex(v)).Select(v => map[v]).ToHashSet();

            return new Attack(
                attackMessage.ID,
                owner,
                target,
                attackMessage.Points,
                territory,
                attackMessage.IsGlobalAttack);
        }
    }
}