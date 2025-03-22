using CustomUtilities.DataStructures;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    // public static class MainMap
    // {
    //     public static GameEvents Events { get; set; }
    //     public static HexMap Map { get; set; }
    //     public static IGameReferee Referee { get; set; }
    //     public static IPhaseManager PhaseManager { get; set; }
    //     public static IAttackInvoker AttackInvoker { get; set; }
    // }
    public class MainMap : MonoBehaviour
    {
        public GameEvents Events { get; set; }
        public HexMap Map { get; set; }
        public IGameReferee Referee { get; set; }
        public IPhaseManager PhaseManager { get; set; }
        public IAttackInvoker AttackInvoker { get; set; }
    }
}