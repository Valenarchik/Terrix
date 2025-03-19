using Terrix.Map;

namespace Terrix.Game.GameRules
{
    public static class MainMap
    {
        public static GameEvents Events { get; set; }
        public static HexMap Map { get; set; }
        public static IGameReferee Referee { get; set; }
        public static IPhaseManager PhaseManager { get; set; }
        public static IAttackInvoker AttackInvoker { get; set; }
    }
}