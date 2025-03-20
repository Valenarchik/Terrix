using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Map
{
    public class Country : Territory
    {
        private readonly IGameDataProvider gameDataProvider;

        /// <summary>
        /// Храню информацию о территориях в сжатом формате
        /// </summary>
        private readonly Dictionary<HexType, int> cellsByTypeCount;

        private readonly HashSet<Hex> cellsSet;

        public int ID { get; }
        public float Population { get; private set; }
        public int TotalCellsCount { get; private set; }

        public float DensePopulation => Population / TotalCellsCount;
        public override IReadOnlyCollection<Hex> Cells => cellsSet;
        public event Action<UpdateCellsData> OnCellsUpdate;


        public Country(int id, [NotNull] IGameDataProvider gameDataProvider, [NotNull] Player owner) : base(
            Enumerable.Empty<Hex>(),
            owner)
        {
            this.ID = id;
            this.gameDataProvider = gameDataProvider;

            cellsByTypeCount = Enum.GetValues(typeof(HexType))
                .OfType<HexType>()
                .ToDictionary(type => type, _ => 0);
        }

        public void CollectIncome()
        {
            var gameData = gameDataProvider.Get();
            var cellsStats = gameData.CellsStats;

            foreach (var (cellType, count) in cellsByTypeCount)
            {
                Population += cellsStats[cellType].Income * count;
            }

            Population = Mathf.Clamp(Population, 0, TotalCellsCount * gameData.MaxDensePopulation);
        }

        public void UpdateCells([NotNull] UpdateCellsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.CountryId != ID)
            {
                throw new InvalidOperationException($"{nameof(Country)}.{nameof(UpdateCells)} | Не верно указан id!");
            }

            foreach (var cell in data.AddedCells)
            {
                ValidateAddCell(cell);
            }

            foreach (var cell in data.RemovedCells)
            {
                ValidateRemoveCell(cell);
            }

            foreach (var cell in data.AddedCells)
            {
                cellsSet.Add(cell);
                cellsByTypeCount[cell.HexType]++;
                TotalCellsCount++;
            }

            foreach (var cell in data.RemovedCells)
            {
                cellsSet.Remove(cell);
                cellsByTypeCount[cell.HexType]--;
                TotalCellsCount--;
            }
            
            OnCellsUpdate?.Invoke(data);
        }

        private void ValidateRemoveCell(Hex hex)
        {
            if (!cellsSet.Contains(hex))
            {
                throw new Exception("Данной клетки нет в этой стране.");
            }
        }

        private void ValidateAddCell(Hex hex)
        {
            if (hex.Owner != Owner)
            {
                throw new Exception("Прежде чем добавлять клетки стране, назначте клеткам владельца.");
            }

            if (cellsSet.Contains(hex))
            {
                throw new Exception("Данная клетка уже добавлена.");
            }
        }

        public class UpdateCellsData
        {
            public int CountryId { get; }
            public Hex[] AddedCells { get; }
            public Hex[] RemovedCells { get; }

            public UpdateCellsData(int countryId, [NotNull] Hex[] addedCells, [NotNull] Hex[] removedCells)
            {
                CountryId = countryId;
                AddedCells = addedCells ?? throw new ArgumentNullException(nameof(addedCells));
                RemovedCells = removedCells ?? throw new ArgumentNullException(nameof(removedCells));
            }
        }
    }
}