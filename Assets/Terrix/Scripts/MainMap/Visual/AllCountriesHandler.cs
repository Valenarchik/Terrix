// using System;
// using System.Collections.Generic;
// using System.Linq;
// using JetBrains.Annotations;
// using Terrix.Map;
// using UnityEngine;
//
// namespace Terrix.Visual
// {
//     // На сервере
//     public class AllCountriesHandler: MonoBehaviour
//     {
//         [SerializeField] private AllCountriesDrawer allCountriesDrawer;
//         
//         private Country[] countries;
//         private bool initialize;
//         private bool handle;
//
//         private List<Country.UpdateCellsData> updatedData;
//         
//         public void Initialize([NotNull] Country[] countries)
//         {
//             this.countries = countries ?? throw new ArgumentNullException(nameof(countries));
//
//
//             initialize = true;
//             
//             SubscribeCountryEvents();
//         }
//
//         private void OnEnable()
//         {
//             SubscribeCountryEvents();
//         }
//
//         private void OnDisable()
//         {
//             UnsubscribeCountryEvents();
//         }
//
//         private void Update()
//         {
//             if (updatedData.Count == 0)
//             {
//                 return;
//             }
//
//
//             var zoneDataList = new List<CountryDrawer.ZoneUpdateData>();
//             var groups = updatedData.GroupBy(data => data.CountryId);
//             foreach (var group in groups)
//             {
//                 var updateCellsDataArray = group.ToArray();
//                 var zoneData = new CountryDrawer.ZoneUpdateData()
//                 {
//                     CountryId = group.Key,
//                     AddedTiles = updateCellsDataArray.SelectMany(data => data.AddedCells).Select(hex => hex.Position).Distinct().ToArray(),
//                     RemovedTiles = updateCellsDataArray.SelectMany(data => data.RemovedCells).Select(hex => hex.Position).Distinct().ToArray()
//                 };
//                 
//                 zoneDataList.Add(zoneData);
//             }
//         }
//
//         private void CountryOnCellsUpdate(Country.UpdateCellsData data)
//         {
//             updatedData.Add(data);
//         }
//         
//         
//         private void SubscribeCountryEvents()
//         {
//             if (!initialize)
//             {
//                 return;
//             }
//
//             if (handle)
//             {
//                 return;
//             }
//             
//             foreach (var country in countries)
//             {
//                 country.OnCellsUpdate += CountryOnCellsUpdate;
//             }
//
//             handle = true;
//         }
//
//         private void UnsubscribeCountryEvents()
//         {
//             if (!initialize)
//             {
//                 return;
//             }
//
//             if (!handle)
//             {
//                 return;
//             }
//
//             foreach (var country in countries)
//             {
//                 country.OnCellsUpdate -= CountryOnCellsUpdate;
//             }
//
//             handle = false;
//         }
//     }
// }