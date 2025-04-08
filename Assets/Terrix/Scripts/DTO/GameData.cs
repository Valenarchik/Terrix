using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

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
        public Dictionary<HexType, Tile> HexTiles { get; }


        public GameData(
            Dictionary<HexType, GameHexData> cellsStats,
            float baseCostOfNeutralLends,
            float tickDurationInSeconds,
            float maxDensePopulation,
            TimeSpan timeForChooseFirstCountryPosition,
            float startCountryPopulation,
            Dictionary<HexType, Tile> hexTiles)
        {
            CellsStats = cellsStats;
            BaseCostOfNeutralLends = baseCostOfNeutralLends;
            TickDurationInSeconds = tickDurationInSeconds;
            MaxDensePopulation = maxDensePopulation;
            TimeForChooseFirstCountryPosition = timeForChooseFirstCountryPosition;
            StartCountryPopulation = startCountryPopulation;
            HexTiles = hexTiles;
        }
    }
}