using System;
using Terrix.DTO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terrix.Settings
{
    public interface IGameDataProvider
    {
        public GameSettings Get();
    }
    
    public class GameDataProvider: IGameDataProvider
    {
        public GameSettings Get()
        {
            if (!Application.isPlaying)
            {
                var gameRoot = Object.FindFirstObjectByType<GameRoot>() ?? throw new Exception($"{nameof(GameRoot)} не найден на сцене!");
                return gameRoot.GameSettingsSo.Get();
            }
            
            return GameRoot.Instance.GameSettingsSo.Get();
        }
    }
}