using System.Collections;
using System.Collections.Generic;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Game.GameRules
{
    public class TickGenerator: MonoBehaviour
    {
        private readonly IGameDataProvider gameDataProvider = new GameDataProvider();

        private readonly List<ITickHandler> tickHandlersOrder = new();

        public void Initialize(IEnumerable<ITickHandler> tickHandlers)
        {
            foreach (var tickHandler in tickHandlers)
            {
                if(tickHandler != null)
                {
                    tickHandlersOrder.Add(tickHandler);
                }
            }

            StartCoroutine(Loop());
        }

        public void AddLast(ITickHandler tickHandler)
        {
            if (tickHandler == null)
            {
                return;
            }
            
            tickHandlersOrder.Add(tickHandler);
        }

        public bool Remove(ITickHandler tickHandler)
        {
            return tickHandlersOrder.Remove(tickHandler);
        }

        private IEnumerator Loop()
        {
            
            foreach (var handler in tickHandlersOrder)
            {
                handler.HandleTick();
            }

            var gameData = gameDataProvider.Get();
            yield return new WaitForSeconds(gameData.TickDurationInSeconds);
        }
    }
}