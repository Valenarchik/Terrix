using System;
using Terrix.Visual;

namespace Terrix.Game.GameRules
{
    public interface IObserverPopulationInfoUpdater : ITickHandler
    {
    }
    public class ObserverPopulationInfoUpdater : IObserverPopulationInfoUpdater
    {
        private readonly AllCountriesHandler allCountriesHandler;

        public ObserverPopulationInfoUpdater(AllCountriesHandler allCountriesHandler)
        {
            this.allCountriesHandler = allCountriesHandler;
        }
        
        public void HandleTick()
        {
            allCountriesHandler.UpdateCountriesPopulation_OnServer();
        }
    }
}