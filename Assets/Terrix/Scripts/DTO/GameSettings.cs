using System;
using System.Collections.Generic;

namespace Terrix.DTO
{
    public class GameSettings
    {
        public Dictionary<HexType, GameHexData> CellsStats { get; }
        public float BaseCostOfNeutralLends { get; }
        public float MaxDensePopulation { get; }
        public float StartCountryPopulation { get; }
        public TimeSpan TimeForChooseFirstCountryPosition { get; }
        public Dictionary<TickHandlerType, TickHandlerSettings> TickHandlers { get; }

        public GameSettings(
            Dictionary<HexType, GameHexData> cellsStats,
            float baseCostOfNeutralLends,
            float maxDensePopulation,
            TimeSpan timeForChooseFirstCountryPosition,
            float startCountryPopulation,
            Dictionary<TickHandlerType, TickHandlerSettings> tickHandlers)
        {
            CellsStats = cellsStats;
            BaseCostOfNeutralLends = baseCostOfNeutralLends;
            MaxDensePopulation = maxDensePopulation;
            TimeForChooseFirstCountryPosition = timeForChooseFirstCountryPosition;
            StartCountryPopulation = startCountryPopulation;
            TickHandlers = tickHandlers;
        }
    }
}