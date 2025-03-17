using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.DTO;

namespace Terrix.Game.GameRules
{
    public interface IPhaseManager
    {
        GamePhaseType CurrentPhase { get; }
        event Action PhaseChanged;
        void NextPhase();
    }

    public class PhaseManager : IPhaseManager
    {
        private static readonly List<GamePhaseType> DefaultPhaseOrder = new()
        {
            GamePhaseType.Uninitialized,
            GamePhaseType.Initial,
            GamePhaseType.Main,
            GamePhaseType.Finish
        };

        private List<GamePhaseType> phaseOrder;
        private int currentPhaseIndex = 0;
        
        public GamePhaseType CurrentPhase => phaseOrder[currentPhaseIndex];
        public event Action PhaseChanged; 

        public PhaseManager(IEnumerable<GamePhaseType> phaseOrder = null)
        {
            this.phaseOrder = phaseOrder != null ? phaseOrder.ToList() : DefaultPhaseOrder;

            if (!this.phaseOrder.Any())
            {
                throw new Exception("PhaseOrder не может быть пустым");
            }
        }

        public void NextPhase()
        {
            if (currentPhaseIndex != phaseOrder.Count - 1)
            {
                currentPhaseIndex++;
                PhaseChanged?.Invoke();
            }
        }
    }
}