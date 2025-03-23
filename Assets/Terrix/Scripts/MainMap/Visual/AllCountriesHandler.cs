using System;
using JetBrains.Annotations;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Visual
{
    // Только на сервере
    public class AllCountriesHandler: MonoBehaviour
    {
        [SerializeField] private AllCountriesDrawer allCountriesDrawer;
        
        private Country[] countries;
        private bool initialize;
        private bool handle;
        
        public void Initialize([NotNull] Country[] countries)
        {
            this.countries = countries ?? throw new ArgumentNullException(nameof(countries));


            initialize = true;
            
            SubscribeCountryEvents();
        }

        private void OnEnable()
        {
            SubscribeCountryEvents();
        }

        private void OnDisable()
        {
            UnsubscribeCountryEvents();
        }
        
        
        private void CountryOnCellsUpdate(Country.UpdateCellsData data)
        {
            allCountriesDrawer.UpdateZone(data);
        }
        
        
        private void SubscribeCountryEvents()
        {
            if (!initialize)
            {
                return;
            }

            if (handle)
            {
                return;
            }
            
            foreach (var country in countries)
            {
                country.OnCellsUpdate += CountryOnCellsUpdate;
            }

            handle = true;
        }

        private void UnsubscribeCountryEvents()
        {
            if (!initialize)
            {
                return;
            }

            if (!handle)
            {
                return;
            }

            foreach (var country in countries)
            {
                country.OnCellsUpdate -= CountryOnCellsUpdate;
            }

            handle = false;
        }
    }
}