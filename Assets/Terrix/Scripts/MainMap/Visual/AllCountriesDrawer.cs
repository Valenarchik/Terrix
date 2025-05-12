using System;
using System.Collections.Generic;
using FishNet.Object;
using JetBrains.Annotations;
using Terrix.Controllers;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Network.DTO;
using Terrix.Networking;
using UnityEngine;

namespace Terrix.Visual
{
    //На сервере тоже
    // Только на клиенте
    public class AllCountriesDrawer : NetworkBehaviour
    {
        public static int DRAG_ZONE_ID = -1;

        [Header("Prefabs")]
        [SerializeField] private CountryDrawer countryDrawerPrefab;
        [SerializeField] private CountryDrawer dragZoneDrawerPrefab;

        [Header("References")]
        [SerializeField] private PlayerCommandsExecutor playerCommandsExecutor;
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
            dragZoneDrawer = Instantiate(dragZoneDrawerPrefab, playerInstantiateRoot.transform, true);
            dragZoneDrawer.Initialize(new CountryDrawer.Settings(settings.DragZone.PlayerId, dragZoneMaterial,
                settings.Zones.Length, ""));
        }

        // [ObserversRpc]
        // public void UpdateZone_ToObserver(Country.UpdateCellsData updateData, float score)
        // {
        //     drawersByIds[updateData.PlayerId].UpdateZone(updateData, score);
        // }
        public void UpdateZone(Country.UpdateCellsData updateData, float score)
        {
            drawersByIds[updateData.PlayerId].UpdateZone(updateData, score);
        }


        public void UpdateDragZone(Country.UpdateCellsData updateData, float score)
        {
            dragZoneDrawer.UpdateZone(updateData, score);
        }

        public void UpdateScore_OnClient(CountryPopulation[] simplifiedCountries)
        {
            foreach (var simplifiedCountry in simplifiedCountries)
            {
                if (simplifiedCountry.PlayerId == DRAG_ZONE_ID)
                {
                    return;
                }

                drawersByIds[simplifiedCountry.PlayerId].UpdateScore(simplifiedCountry.Population);
            }
        }

        public class Settings
        {
            public ZoneData[] Zones { get; set; }
            public ZoneData DragZone { get; set; }

            public Settings(ZoneData[] zones, ZoneData dragZone)
            {
                Zones = zones;
                DragZone = dragZone;
            }
        }
    }
}