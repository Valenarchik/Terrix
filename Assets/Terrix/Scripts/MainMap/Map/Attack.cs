using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Entities;

namespace Terrix.Map
{
    public class Attack: Entity<int>
    {
        public Player Owner { get; }
        public HashSet<Hex> Territory { get;  }
        public float Points { get;  }
        
        [MaybeNull] public Player Target { get; }
        
        public Attack(int id, Player owner, HashSet<Hex> territory, float points, Player target): base(id)
        {
            Owner = owner;
            Territory = territory;
            Points = points;
            Target = target;
        }
    }
}