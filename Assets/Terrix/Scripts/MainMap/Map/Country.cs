using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Map
{
    public class Country: IEnumerable<Hex>
    {
        private readonly IGameDataProvider gameDataProvider;
        private readonly HexMap map;
        
        private readonly Dictionary<HexType, int> cellsByTypeCount;
        private readonly HashSet<Hex> cellsSet = new();

        public int PlayerId => Owner.ID;
        public float Population { get; private set; }
        public int TotalCellsCount { get; private set; }
        public float DensePopulation { get; private set; }
        public HashSet<Hex> Border { get; }
        public Player Owner { get; private set; }
        
        public event Action<UpdateCellsData> OnCellsUpdate;


        public Country([NotNull] IGameDataProvider gameDataProvider, [NotNull] Player owner, HexMap map)
        {
            this.gameDataProvider = gameDataProvider;
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));

            cellsByTypeCount = Enum.GetValues(typeof(HexType))
                .OfType<HexType>()
                .ToDictionary(type => type, _ => 0);

            Population = gameDataProvider.Get().StartCountryPopulation;
            DensePopulation = 0;
            Border = new HashSet<Hex>();
            this.map = map;
        }

        public bool Contains(Hex cell)
        {
            return cellsSet.Contains(cell);
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
            if (TotalCellsCount != 0)
            {
                DensePopulation = Population / TotalCellsCount;
            }
            else
            {
                DensePopulation = 0;
            }
        }

        public void ClearAndAdd(IEnumerable<Hex> addedHexesAfterClear)
        {
            RemoveAndAdd(cellsSet, addedHexesAfterClear);
        }

        public void RemoveAndAdd(IEnumerable<Hex> removedHexes, IEnumerable<Hex> addedHexes)
        {
            var removedSet = removedHexes.ToHashSet();
            var addedSet = addedHexes.ToHashSet();

            var changeData = new List<CellChangeData>(removedSet.Count + addedSet.Count);
            changeData.AddRange(removedSet.Select(hex => new CellChangeData(hex, UpdateCellMode.Remove)));
            changeData.AddRange(addedSet.Select(hex => new CellChangeData(hex, UpdateCellMode.Add)));
            
            var data = new UpdateCellsData(PlayerId, changeData.ToArray());

            UpdateCells(data);
        }

        public void UpdateCells([NotNull] UpdateCellsData data)
        {
            ValidateUpdateCellsData(data);
            
            foreach (var updateData in data.ChangeData)
            {
                var cell = updateData.Hex;
                switch (updateData.Mode)
                {
                    case UpdateCellMode.Add:
                    {
                        if (cellsSet.Add(cell))
                        {
                            cellsByTypeCount[cell.HexType]++;
                            TotalCellsCount++;
                            cell.PlayerId = PlayerId;
                        }
                        break;
                    }
                    case UpdateCellMode.Remove:
                    {
                        if (cellsSet.Remove(cell))
                        {
                            cellsByTypeCount[cell.HexType]--;
                            TotalCellsCount--;
                            cell.PlayerId = null;
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }

            UpdateBorder();
            OnCellsUpdate?.Invoke(data);
        }

        private void UpdateBorder()
        {
            Border.Clear();
            foreach (var cell in cellsSet)
            {
                if (cell.GetNeighbours(map).Any(neighbor => !Contains(neighbor)))
                {
                    Border.Add(cell);
                }
            }
        }

        private void ValidateUpdateCellsData(UpdateCellsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.PlayerId != PlayerId)
            {
                throw new InvalidOperationException($"{nameof(Country)}.{nameof(UpdateCells)} | Не верно указан id!");
            }
        }

        public class UpdateCellsData
        {
            public int PlayerId { get; }
            public CellChangeData[] ChangeData { get; }

            public UpdateCellsData(int playerId, [NotNull] CellChangeData[] changeData)
            {
                PlayerId = playerId;
                ChangeData = changeData ?? throw new ArgumentNullException(nameof(changeData));
            }
        }

        public struct CellChangeData
        {
            public Hex Hex { get; set; }
            public UpdateCellMode Mode { get; set; }

            public CellChangeData(Hex hex, UpdateCellMode mode)
            {
                Hex = hex;
                Mode = mode;
            }
        }

        public enum UpdateCellMode
        {
            Add,
            Remove
        }

        public IEnumerator<Hex> GetEnumerator()
        {
            return cellsSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}