using System.Collections.Generic;
using System.Linq;
using CustomUtilities.Attributes;
using Terrix.DTO;
using UnityEngine;

namespace Terrix.Settings
{
    [CreateAssetMenu(menuName = "Game settings/Game data")]
    public class GameDataSO : ScriptableObject
    {
        [NamedArray("HexType")] public List<HexDataSerializable> HexStats;
        public float BaseCostOfNeutralLends;

        public GameData Get()
        {
            return new GameData(
                HexStats.ToDictionary(stats => stats.HexType, stats => stats.Get()),
                BaseCostOfNeutralLends);
        }
    }
}