using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Terrix.Visual
{
    // На клиенте
    public class AllCountriesDrawer: MonoBehaviour
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

        public void UpdateZones(ZonesUpdateData data)
        {
            foreach (var updateData in data.Data)
            {
                drawersByIds[updateData.CountryId].UpdateZone(updateData);
            }
        }
        
        public class Settings
        {
            public ZoneData[] Zones { get; }
            public Settings(ZoneData[] zones)
            {
                Zones = zones;
            }
        }

        public class ZonesUpdateData
        {
            public CountryDrawer.ZoneUpdateData[] Data { get; set; } = Array.Empty<CountryDrawer.ZoneUpdateData>();
        }
    }
}