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
        public HashSet<Hex> Territory { get; }
        public float Points { get;  }
        
        [MaybeNull] public Player Target { get; }
        
        public Attack(Guid id, Player owner, Player target, float points, HashSet<Hex> territory) : base(id)
        {
            Owner = owner;
            Territory = territory;
            Points = points;
            Target = target;
        }
    }
}