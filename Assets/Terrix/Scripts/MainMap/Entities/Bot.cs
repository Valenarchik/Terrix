using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    /// <summary>
    /// ИИ который может управлять игроком
    /// </summary>
    public class Bot : Player
    {
        public Bot(int id, PlayerType playerType) : base(id, playerType)
        {
        }

        public Bot(int id, PlayerType playerType, Country country, string playerName) : base(id, playerType, country,
            playerName)
        {
        }
    }
}