using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terrix.Entities;

namespace Terrix.Map
{
    public class Attack: IEquatable<Attack>
    {
        public int ID { get; }
        public Player Owner { get; }
        public HashSet<Hex> Territory { get;  }
        public float Points { get;  }
        
        [MaybeNull] public Player Target { get; }
        
        public Attack(int id, Player owner, HashSet<Hex> territory, float points, Player target)
        {
            ID = id;
            Owner = owner;
            Territory = territory;
            Points = points;
            Target = target;
        }

        public bool Equals(Attack other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Attack) obj);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}