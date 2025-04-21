using System;


namespace Terrix.Game.GameRules
{
    public interface IGame
    {
        public bool GameOver { get; }
        public bool Started { get; }
        
        void OnGameStarted(Action action);
        void OnGameOver(Action action);
        void StartGame();
        void FinishGame();
    }

    public class Game : IGame
    {
        private bool gameIsReady;
        private Action onGameReady;
        
        private bool gameIsFinished;
        private Action onGameFinished;

        public bool Started => gameIsReady;
        public bool GameOver => gameIsFinished;

        public void OnGameStarted(Action action)
        {
            if (gameIsReady)
            {
                action?.Invoke();
                return;
            }

            this.onGameReady += action;
        }

        public void OnGameOver(Action action)
        {
            if (gameIsFinished)
            {
                action?.Invoke();
                return;
            }

            this.onGameFinished += action;
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

        public void FinishGame()
        {
            if (gameIsFinished)
            {
                return;
            }
            gameIsFinished = true;
            onGameFinished?.Invoke();
        }
    }
}