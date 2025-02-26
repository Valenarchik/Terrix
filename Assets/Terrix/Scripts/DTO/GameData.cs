using System;
using System.Collections.Generic;

namespace Terrix.DTO
{
    public class GameData
    {
        public Dictionary<HexType, HexData> CellsStats { get; }
        public float BaseCostOfNeutralLends { get; }

        public GameData(
            Dictionary<HexType, HexData> cellsStats,
            float baseCostOfNeutralLends)
        {
            CellsStats = cellsStats;
            BaseCostOfNeutralLends = baseCostOfNeutralLends;
        }
    }
}