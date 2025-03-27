using System;
using System.Collections.Generic;
using FishNet.Object;
using JetBrains.Annotations;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Visual
{
    // Только на клиенте
    public class AllCountriesDrawer : NetworkBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private CountryDrawer countryDrawerPrefab;

        [Header("References")]
        [SerializeField] private ZoneMaterialFactory zoneMaterialFactory;
        [SerializeField] private GameObject playerInstantiateRoot;

        private Dictionary<int, CountryDrawer> drawersByIds;

        public void Initialize([NotNull] Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            drawersByIds = new Dictionary<int, CountryDrawer>();

            foreach (var zone in settings.Zones)
            {
                var material = zoneMaterialFactory.Create(zone);
                var countryDrawer = Instantiate(countryDrawerPrefab, playerInstantiateRoot.transform, true);
                drawersByIds.Add(zone.ID, countryDrawer);
                countryDrawer.Initialize(new CountryDrawer.Settings(zone.ID, material));
            }
        }

        [ObserversRpc]
        public void UpdateZone(Country.UpdateCellsData updateData)
        {
            drawersByIds[updateData.PlayerId].UpdateZone(updateData);
        }

        public class Settings
        {
            public ZoneData[] Zones { get; }

            public Settings(ZoneData[] zones)
            {
                Zones = zones;
            }
        }
    }
}