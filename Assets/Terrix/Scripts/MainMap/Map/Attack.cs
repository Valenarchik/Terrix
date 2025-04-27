using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Entities;

namespace Terrix.Map
{
    public class Attack: Entity<Guid>
    {
        public Player Owner { get; }
        public bool IsGlobalAttack { get; set; }
        [MaybeNull] public HashSet<Hex> Territory { get; }
        public float Points { get;  }
        
        [MaybeNull] public Player Target { get; }
        
        public Attack(Guid id, Player owner, Player target, float points, HashSet<Hex> territory, bool isGlobalAttack) : base(id)
        {
            Owner = owner;
            Territory = territory;
            Points = points;
            Target = target;
            IsGlobalAttack = isGlobalAttack;
        }
    }

    public class AttackBuilder
    {
        public Player Owner { get; set; }
        public bool IsGlobalAttack { get; set; }
        public HashSet<Hex> Territory { get; set; }
        public float Points { get; set; }
        public Player Target { get; set; }
        
        public Attack Build() => new(Guid.NewGuid(), Owner, Target, Points, Territory, IsGlobalAttack);
    }
}