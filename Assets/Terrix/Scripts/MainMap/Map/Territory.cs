using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.Entities;

namespace Terrix.Map
{
    public class Territory
    {
        public virtual IReadOnlyCollection<Hex> Cells => cells;
        private readonly Hex[] cells;

        public Territory(IEnumerable<Hex> cells)
        {
            this.cells = cells.ToArray();
        }
    }
}