using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using CustomUtilities.Attributes;
using Terrix.DTO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Terrix.Settings
{
    [CreateAssetMenu(menuName = "Game settings/Game data")]
    public class GameDataSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [NamedArray("HexType")] public List<HexDataSerializable> HexStats;
        public float BaseCostOfNeutralLends;
        public float TickDurationInSeconds = 0.1f;
        public float MaxDensePopulation;
        public float TimeForChooseFirstCountryPositionInSeconds = 30;
        [SerializedDictionary] public SerializedDictionary<HexType, Tile> HexTiles;

        private GameData gameData;

        public GameData Get()
        {
            return gameData;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            gameData = new GameData(
                HexStats.ToDictionary(stats => stats.HexType, stats => stats.Get()),
                BaseCostOfNeutralLends,
                TickDurationInSeconds,
                MaxDensePopulation,
                TimeSpan.FromSeconds(TimeForChooseFirstCountryPositionInSeconds),
                HexTiles);
        }
    }
}