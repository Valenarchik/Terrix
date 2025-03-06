using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Settings;

namespace Terrix.Map
{
    public class Country: Territory
    {
        private readonly IGameDataProvider gameDataProvider;
        /// <summary>
        /// Храню информацию о территориях в сжатом формате
        /// </summary>
        private readonly Dictionary<HexType, int> cellsByTypeCount;
        private readonly HashSet<Hex> cellsSet;
        public float Population { get; private set; }
        public int TotalCellsCount { get; private set; }

        public float DensePopulation => Population / TotalCellsCount;

        public override IReadOnlyCollection<Hex> Cells => cellsSet;


        public Country([NotNull] IGameDataProvider gameDataProvider, [NotNull] Player owner): base(Enumerable.Empty<Hex>(), owner)
        {
            this.gameDataProvider = gameDataProvider;

            cellsByTypeCount = Enum.GetValues(typeof(HexType))
                .OfType<HexType>()
                .ToDictionary(type => type, _ => 0);
        }
        
        public void CollectIncome()
        {
            var cellsStats = gameDataProvider.Get().CellsStats;
                
            foreach (var (cellType, count) in cellsByTypeCount)
            {
                Population += cellsStats[cellType].Income * count;
            }
        }

        public void AddCells(IEnumerable<Hex> cells)
        {
            cells ??= Enumerable.Empty<Hex>();
            cells = cells.ToArray();

            foreach (var cell in cells)
            {
                ValidateSell(cell);
            }
            
            foreach (var cell in cells)
            {
                cellsSet.Add(cell);
                cellsByTypeCount[cell.HexType]++;
                TotalCellsCount++;
            }
        }

        public void RemoveCells(IEnumerable<Hex> cells)
        {
            cells ??= Enumerable.Empty<Hex>();
            foreach (var cell in cells)
            {
                if (cellsSet.Remove(cell))
                {
                    cellsByTypeCount[cell.HexType]--;
                    TotalCellsCount--;
                }
            }
        }

        private void ValidateSell(Hex hex)
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
    }
}