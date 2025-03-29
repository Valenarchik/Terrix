using System;
using System.Diagnostics.CodeAnalysis;
using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    public class Player: IEquatable<Player>
    {
        public int ID { get; }
        public PlayerType PlayerType { get; }
        [MaybeNull] public Country Country { get; set; }
        
        public Player(int id, PlayerType playerType)
        {
            ID = id;
            PlayerType = playerType;
        }

        public void Lose()
        {
            Country = null;
        }

        public bool Equals(Player other)
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

            return Equals((Player) obj);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}