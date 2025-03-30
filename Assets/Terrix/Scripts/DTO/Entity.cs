using System.Collections.Generic;

namespace Terrix.DTO
{
    public abstract class Entity<TId>
    {
        public TId ID { get; }
        
        public Entity(TId id)
        {
            ID = id;
        }
        
        protected bool Equals(Entity<TId> other)
        {
            return EqualityComparer<TId>.Default.Equals(ID, other.ID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Entity<TId>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TId>.Default.GetHashCode(ID);
        }

        public override string ToString()
        {
            return $"{GetType().Name}({nameof(ID)}: {ID})";
        }
    }
}