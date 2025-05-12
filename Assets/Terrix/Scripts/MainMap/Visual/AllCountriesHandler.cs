using System;
using System.Linq;
using FishNet.Object;
using JetBrains.Annotations;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Network.DTO;
using Terrix.Networking;
using UnityEngine;
using UnityEngine.Serialization;

namespace Terrix.Visual
{
    // Только на сервере и на клиенте
    public class AllCountriesHandler : NetworkBehaviour
    {
        [SerializeField] private AllCountriesDrawer allCountriesDrawer;
        [SerializeField] private TickGenerator tickGenerator;
        [SerializeField] private GameUI gameUI;
        // [SerializeField] private LeaderboardUI leaderboardUI;


        private IPlayersProvider players;
        private HexMap hexMap;


        private Country[] countries;
        private bool initialize;
        private bool handle;


        // public override void OnStartServer()
        // {
        //     tickGenerator.OnUpdated += TickGenerator_OnUpdated;
        // }

        // private void TickGenerator_OnUpdated()
        // {
        //     Debug.Log("Fixed tick");
        //     UpdateCountriesPopulation_ToObserver(countries
        //         .Select(country => new SimplifiedCountry(country.PlayerId, country.Population, country.TotalCellsCount))
        //         .ToArray());
        // }
        public void UpdateCountriesPopulation_OnServer()
        {
            UpdateCountriesPopulation_ToObserver(countries
            .Select(country => new CountryPopulation(country.PlayerId, country.Population))
            .ToArray());
        }

        [ObserversRpc]
        private void UpdateCountriesPopulation_ToObserver(
            CountryPopulation[] simplifiedCountries)
        {
            foreach (var simplifiedCountry in simplifiedCountries)
            {
                players.Find(simplifiedCountry.PlayerId).Country.Population = simplifiedCountry.Population;
            }

            allCountriesDrawer.UpdateScore_OnClient(simplifiedCountries);
            gameUI.UpdateInfo();
        }

        public void Initialize_OnServer([NotNull] IPlayersProvider players)
        {
            this.players = players ?? throw new ArgumentNullException(nameof(players));
            this.countries = players.GetAll().Select(player => player.Country).ToArray();

            initialize = true;
            SubscribeCountryEvents();
        }

        public void Initialize_OnClient([NotNull] IPlayersProvider players, [NotNull] HexMap hexMap)
        {
            this.players = players ?? throw new ArgumentNullException(nameof(players));
            this.hexMap = hexMap ?? throw new ArgumentNullException(nameof(players));
            initialize = true;
        }

        private void OnDisable()
        {
            UnsubscribeCountryEvents();
        }


        private void CountryOnCellsUpdate(Country.UpdateCellsData data)
        {
            UpdateCountries_ToObserver(data);
        }

        [ObserversRpc]
        private void UpdateCountries_ToObserver(Country.UpdateCellsData data)
        {
            var hexes = data.ChangeData.Select(changeData => changeData.Hex).ToArray();
            foreach (var hex in hexes)
            {
                hexMap.FindHex(hex.Position).PlayerId = hex.PlayerId;
                hex.HexMap = hexMap;
            }

            players.Find(data.PlayerId).Country.UpdateCells_OnClient(data);
            allCountriesDrawer.UpdateZone(data, 1);
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