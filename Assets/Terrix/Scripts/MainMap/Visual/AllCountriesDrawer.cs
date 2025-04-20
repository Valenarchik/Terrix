using System;
using System.Collections.Generic;
using FishNet.Object;
using JetBrains.Annotations;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Networking;
using UnityEngine;

namespace Terrix.Visual
{
    // Только на клиенте
    public class AllCountriesDrawer : NetworkBehaviour
    {
        public static int DRAG_ZONE_ID = -1;

        [Header("Prefabs")]
        [SerializeField] private CountryDrawer countryDrawerPrefab;

        [Header("References")]
        [SerializeField] private ZoneMaterialFactory zoneMaterialFactory;
        [SerializeField] private GameObject playerInstantiateRoot;

        private Dictionary<int, CountryDrawer> drawersByIds;
        private CountryDrawer dragZoneDrawer;

        public void Initialize([NotNull] Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            drawersByIds = new Dictionary<int, CountryDrawer>();

            for (var i = 0; i < settings.Zones.Length; i++)
            {
                var zone = settings.Zones[i];
                var material = zoneMaterialFactory.Create(zone);
                var countryDrawer = Instantiate(countryDrawerPrefab, playerInstantiateRoot.transform, true);
                drawersByIds.Add(zone.PlayerId, countryDrawer);
                countryDrawer.Initialize(new CountryDrawer.Settings(zone.PlayerId, material, i + 1, zone.PlayerName));
            }

            var dragZoneMaterial = zoneMaterialFactory.Create(settings.DragZone);
            dragZoneDrawer = Instantiate(countryDrawerPrefab, playerInstantiateRoot.transform, true);
            dragZoneDrawer.Initialize(new CountryDrawer.Settings(settings.DragZone.PlayerId, dragZoneMaterial,
                settings.Zones.Length, ""));
        }

        [ObserversRpc]
        public void UpdateZone_ToObserver(Country.UpdateCellsData updateData, float score)
        {
            drawersByIds[updateData.PlayerId].UpdateZone(updateData, score);
        }

        // [ServerRpc(RequireOwnership = false)]
        // public void UpdateZone_ToServer(Country.UpdateCellsData updateData, float score)
        // {
        //     UpdateZone_ToObserver(updateData, score);
        // }

        public void UpdateDragZone(Country.UpdateCellsData updateData, float score)
        {
            dragZoneDrawer.UpdateZone(updateData, score);
        }

        // public void UpdateScore_OnClient(NetworkSerialization.PlayersCountryMapData playersCountryMapData)
        public void UpdateScore_OnClient(SimplifiedCountry[] simplifiedCountries)
        {
            foreach (var simplifiedCountry in simplifiedCountries)
            {
                if (simplifiedCountry.PlayerId == -1)
                {
                    return;
                }
                drawersByIds[simplifiedCountry.PlayerId].UpdateScore(simplifiedCountry.Population);
            }
            // foreach (var drawer in drawersByIds)
            // {
            //     if (drawer.Key == -1)
            //     {
            //         return;
            //     }
            //
            //     drawer.Value.UpdateScore(playersCountryMapData.IPlayersProvider.Find(drawer.Key).Country.Population);
            // }
        }

        public class Settings
        {
            public ZoneData[] Zones { get; }
            public ZoneData DragZone { get; }

            public Settings(ZoneData[] zones, ZoneData dragZone)
            {
                Zones = zones;
                DragZone = dragZone;
            }
        }
    }
}