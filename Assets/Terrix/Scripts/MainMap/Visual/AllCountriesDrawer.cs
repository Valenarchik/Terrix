using System;
using System.Collections.Generic;
using CustomUtilities.CreationCallBack;
using JetBrains.Annotations;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Visual
{
    // Только на клиенте
    public class AllCountriesDrawer: MonoBehaviour
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
                drawersByIds.Add(zone.ID, countryDrawer);
                countryDrawer.Initialize(new CountryDrawer.Settings(zone.ID, material, i));
            }

            var dragZoneMaterial = zoneMaterialFactory.Create(settings.DragZone);
            dragZoneDrawer = Instantiate(countryDrawerPrefab, playerInstantiateRoot.transform, true);
            dragZoneDrawer.Initialize(new CountryDrawer.Settings(settings.DragZone.ID, dragZoneMaterial, settings.Zones.Length));
        }

        public void UpdateZone(Country.UpdateCellsData updateData)
        {
            drawersByIds[updateData.PlayerId].UpdateZone(updateData);
        }

        public void UpdateDragZone(Country.UpdateCellsData updateData)
        {
            dragZoneDrawer.UpdateZone(updateData);
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