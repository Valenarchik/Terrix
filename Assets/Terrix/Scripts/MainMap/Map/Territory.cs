using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.Entities;

namespace Terrix.Map
{
    public class Territory
    {
        public Player Owner { get; }
        public virtual IReadOnlyCollection<Hex> Cells => cells;
        private Hex[] cells;

        public Territory(IEnumerable<Hex> cells, Player owner = null)
        {
            this.cells = cells.ToArray();
            this.Owner = owner;
            
            Validate();
        }

        private void Validate()
        {
            if (cells.Any(cell => cell.Owner != Owner))
            {
                throw new Exception("У территории может быть только один владелец.");
            }
        }
    }
}