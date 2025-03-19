using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Terrix.DTO
{
    public class GameData
    {
        public Dictionary<HexType, HexData> CellsStats { get; }
        public float BaseCostOfNeutralLends { get; }
        public float TickDurationInSeconds { get; }
        public float MaxDensePopulation { get; }

        public TimeSpan TimeForChooseFirstCountryPosition { get; }
        public Dictionary<HexType, Tile> HexTiles { get; }


        public GameData(
            Dictionary<HexType, HexData> cellsStats,
            float baseCostOfNeutralLends,
            float tickDurationInSeconds,
            float maxDensePopulation,
            TimeSpan timeForChooseFirstCountryPosition,
            Dictionary<HexType, Tile> hexTiles)
        {
            CellsStats = cellsStats;
            BaseCostOfNeutralLends = baseCostOfNeutralLends;
            TickDurationInSeconds = tickDurationInSeconds;
            MaxDensePopulation = maxDensePopulation;
            TimeForChooseFirstCountryPosition = timeForChooseFirstCountryPosition;
            HexTiles = hexTiles;
        }
    }
}