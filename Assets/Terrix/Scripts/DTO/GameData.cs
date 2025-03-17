using System;
using System.Collections.Generic;

namespace Terrix.DTO
{
    public class GameData
    {
        public Dictionary<HexType, HexData> CellsStats { get; }
        public float BaseCostOfNeutralLends { get; }
        public float TickDurationInSeconds { get; }
        public float MaxDensePopulation { get; }
        
        public TimeSpan TimeForChooseFirstCountryPosition { get; }

        public GameData(
            Dictionary<HexType, HexData> cellsStats,
            float baseCostOfNeutralLends,
            float tickDurationInSeconds,
            float maxDensePopulation,
            TimeSpan timeForChooseFirstCountryPosition)
        {
            CellsStats = cellsStats;
            BaseCostOfNeutralLends = baseCostOfNeutralLends;
            TickDurationInSeconds = tickDurationInSeconds;
            MaxDensePopulation = maxDensePopulation;
            TimeForChooseFirstCountryPosition = timeForChooseFirstCountryPosition;
        }
    }
}