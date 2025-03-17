using Terrix.DTO;
using Terrix.Map;

namespace Terrix.Entities
{
    /// <summary>
    /// ИИ который может управлять игроком
    /// </summary>
    public class Bot: Player
    {
        public Bot(PlayerType playerType) : base(playerType)
        {
        }
    }
}