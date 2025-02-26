using Terrix.DTO;

namespace Terrix.Settings
{
    public interface IGameDataProvider
    {
        public GameData Get();
    }
    
    public class GameDataProvider: IGameDataProvider
    {
        public GameData Get()
        {
            return GameRoot.Instance.GameDataSo.Get();
        }
    }
}