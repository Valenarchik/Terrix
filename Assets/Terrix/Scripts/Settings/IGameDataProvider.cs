using System;
using Terrix.DTO;
using UnityEngine;
using Object = UnityEngine.Object;

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
            if (!Application.isPlaying)
            {
                var gameRoot = Object.FindFirstObjectByType<GameRoot>() ?? throw new Exception($"{nameof(GameRoot)} не найден на сцене!");
                return gameRoot.GameDataSo.Get();
            }
            
            return GameRoot.Instance.GameDataSo.Get();
        }
    }
}