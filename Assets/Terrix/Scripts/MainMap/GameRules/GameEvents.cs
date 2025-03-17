using System;
using CustomUtilities.DataStructures;

namespace Terrix.Game.GameRules
{
    public class GameEvents : Singleton<GameEvents>
    {
        private bool gameIsReady;
        private Action onGameReady;

        public void OnGameReady(Action action)
        {
            if (gameIsReady)
            {
                action?.Invoke();
                return;
            }

            this.onGameReady += action;
        }

        public void StartGame()
        {
            if (gameIsReady)
            {
                return;
            }
            gameIsReady = true;
            onGameReady?.Invoke();
        }
    }
}