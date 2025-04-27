using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using CustomUtilities.Attributes;
using Terrix.DTO;
using Terrix.MainMap.AI;
using UnityEngine;

namespace Terrix.Settings
{
    [CreateAssetMenu(menuName = "Game settings/Game settings")]
    public class GameSettingsSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [NamedArray("HexType")] public List<HexDataSerializable> HexStats;
        public float BaseCostOfNeutralLends;
        public float MaxDensePopulation = 500;
        public float TimeForChooseFirstCountryPositionInSeconds = 30;
        public float StartCountryPopulation = 100;
        public SerializedDictionary<TickHandlerType, TickHandlerSettingsSerializable> TickHandlers;
        public BotSettings BotSettings;

        private GameSettings gameSettings;

        public GameSettings Get()
        {
            return gameSettings;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            gameSettings = new GameSettings(
                HexStats.ToDictionary(stats => stats.HexType, stats => stats.Get()),
                BaseCostOfNeutralLends,
                MaxDensePopulation,
                TimeSpan.FromSeconds(TimeForChooseFirstCountryPositionInSeconds),
                StartCountryPopulation,
                TickHandlers.ToDictionary(kvp => kvp.Key,
                    kvp => new TickHandlerSettings(
                        kvp.Key,
                        TimeSpan.FromSeconds(kvp.Value.TickDeltaInSeconds),
                        kvp.Value.Priority)),
                BotSettings
            );
        }
    }

    [Serializable]
    public class HexDataSerializable
    {
        public HexType HexType;
        public float Income;
        public float Resist;
        public bool CanCapture;
        public bool IsSeeTile;


        public GameHexData Get()
        {
            return new GameHexData(HexType, Income, Resist, CanCapture, IsSeeTile);
        }
    }

    [Serializable]
    public class TickHandlerSettingsSerializable
    {
        public float TickDeltaInSeconds = 1;
        public int Priority = 0;
    }
}