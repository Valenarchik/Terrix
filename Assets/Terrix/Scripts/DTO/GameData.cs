using System;
using System.Collections.Generic;

namespace Terrix.DTO
{
    public class GameData
    {
        public Dictionary<HexType, GameHexData> CellsStats { get; }
        public float BaseCostOfNeutralLends { get; }
        public float TickDurationInSeconds { get; }
        public float MaxDensePopulation { get; }
        public TimeSpan TimeForChooseFirstCountryPosition { get; }
        public float StartCountryPopulation { get; }

        public GameData(
            Dictionary<HexType, GameHexData> cellsStats,
            float baseCostOfNeutralLends,
            float tickDurationInSeconds,
            float maxDensePopulation,
            TimeSpan timeForChooseFirstCountryPosition,
            float startCountryPopulation)
        {
            CellsStats = cellsStats;
            BaseCostOfNeutralLends = baseCostOfNeutralLends;
            TickDurationInSeconds = tickDurationInSeconds;
            MaxDensePopulation = maxDensePopulation;
            TimeForChooseFirstCountryPosition = timeForChooseFirstCountryPosition;
            StartCountryPopulation = startCountryPopulation;
        }
    }
}