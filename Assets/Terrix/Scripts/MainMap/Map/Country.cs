using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;
using MoreLinq;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Map
{
    public class Country : IEnumerable<Hex>
    {
        private readonly IGameDataProvider gameDataProvider;

        private readonly Dictionary<HexType, int> cellsByTypeCount;

        private readonly HashSet<Hex> cellsSet = new ();
        private readonly HashSet<Hex> innerBorder = new();
        private readonly HashSet<Hex> outerBorder = new();
        
        private readonly HashSet<Hex> updatedCells = new();
        
        private float population;

        public int PlayerId => Owner.ID;

        public float Population
        {
            get => population;
            set
            {
                population = value;
                AssignPopulation();
            }
        }

        public int TotalCellsCount { get; private set; }
        public float DensePopulation { get; private set; }
        public int MaxCellsCount { get; set; }
        public IEnumerable<Hex> Cells => cellsSet;
        public Player Owner { get; private set; }
        public Modifiers CustomModifiers { get; set; }
        public event Action<UpdateCellsData> OnCellsUpdate;

        public Country([NotNull] IGameDataProvider gameDataProvider, [NotNull] Player owner)
        {
            this.gameDataProvider = gameDataProvider;
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));

            cellsByTypeCount = Enum.GetValues(typeof(HexType))
                .OfType<HexType>()
                .ToDictionary(type => type, _ => 0);

            // Population = gameDataProvider.Get().StartCountryPopulation;
            population = gameDataProvider.Get().StartCountryPopulation;
            DensePopulation = 0;
            MaxCellsCount = 0;
            Debug.Log(Population);
        }

        //TODO возможно ошибка
        public Country(IEnumerable<Hex> cellsSet, float population,
            int totalCellsCount, float densePopulation)
        {
            this.gameDataProvider = new GameDataProvider();
            // this.gameDataProvider = gameDataProvider;
            this.cellsSet = cellsSet.ToHashSet();
            cellsByTypeCount = Enum.GetValues(typeof(HexType))
                .OfType<HexType>()
                .ToDictionary(type => type, _ => 0);

            Population = population;
            TotalCellsCount = totalCellsCount;
            DensePopulation = densePopulation;
            // this.map = map;
        }

        public bool Contains(Hex cell)
        {
            return cellsSet.Contains(cell);
        }

        public void CollectIncome()
        {
            // var gameData = gameDataProvider.Get();
            // var cellsStats = gameData.CellsStats;
            //
            // var sum = 0f;
            // foreach (var (cellType, count) in cellsByTypeCount)
            // {
            //     sum += cellsStats[cellType].Income * count;
            // }

            Population += GetIncome();
        }

        public float GetIncome()
        {
            var gameData = gameDataProvider.Get();
            var cellsStats = gameData.CellsStats;
            var sum = 0f;
            var minModifier = DensePopulation / gameDataProvider.Get().MaxDensePopulation * 10;
            var maxModifier = DensePopulation / gameDataProvider.Get().MaxDensePopulation / 2;
            var modifier = 0f;
            modifier = Mathf.Clamp(modifier, minModifier, maxModifier);
            foreach (var (cellType, count) in cellsByTypeCount)
            {
                sum += cellsStats[cellType].Income * modifier * count;
                // sum += cellsStats[cellType].Income * count;
            }
            // sum *= GetModifiers().IncomeMultiplier;
            return sum;
        }

        public void Add(IEnumerable<Hex> added)
        {
            RemoveAndAdd(Enumerable.Empty<Hex>(), added);
        }

        public void Remove(IEnumerable<Hex> removed)
        {
            RemoveAndAdd(removed, Enumerable.Empty<Hex>());
        }

        public void ClearAndAdd(IEnumerable<Hex> addedHexesAfterClear)
        {
            RemoveAndAdd(cellsSet, addedHexesAfterClear);
        }

        public void Clear()
        {
            ClearAndAdd(Array.Empty<Hex>());
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

                            updatedCells.Add(cell);
                            cell.GetNeighbours().ForEach(h => updatedCells.Add(h));
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
                            
                            updatedCells.Add(cell);
                            cell.GetNeighbours().ForEach(h => updatedCells.Add(h));
                        }

                        break;
                    }
                }
            }

            MaxCellsCount = Mathf.Max(TotalCellsCount, MaxCellsCount);
            AssignPopulation();
            OnCellsUpdate?.Invoke(data);
        }
        //TODO убрать
        // public HashSet<Hex> GetInnerBorder()
        // {
        //     if (innerBorderUpdated)
        //     {
        //         innerBorder.Clear();
        //         foreach (var cell in cellsSet)
        //         {
        //             if (cell.GetNeighbours().Any(neighbor => !Contains(neighbor)))
        //             {
        //                 innerBorder.Add(cell);
        //             }
        //         }
        //     }
        //
        //     return innerBorder.ToHashSet();
        // }
        //
        // public HashSet<Hex> GetOuterBorder()
        // {
        //     if (outerBorderUpdated)
        //     {
        //         outerBorder.Clear();
        //         foreach (var hex in GetInnerBorder().SelectMany(hex => hex.GetNeighbours()))
        //         {
        //             outerBorder.Add(hex);
        //         }
        //     
        //         outerBorder.ExceptWith(cellsSet);
        //     }
        //
        //     return outerBorder.ToHashSet();
        // }
        public IReadOnlyCollection<Hex> GetInnerBorder()
        {
            TryApplyUpdates();
            return innerBorder;
        }

        public IReadOnlyCollection<Hex> GetOuterBorder()
        {
            TryApplyUpdates();
            return outerBorder;
        }

        private void TryApplyUpdates()
        {
            if (updatedCells.Count == 0)
            {
                return;
            } 
            
            updatedCells.ForEach(ApplyUpdatedCell);
            updatedCells.Clear();
        }
        
        private void ApplyUpdatedCell(Hex cell)
        {
            if (cell.PlayerId == PlayerId && cell.GetNeighbours().Any(h => h.PlayerId != PlayerId))
            {
                innerBorder.Add(cell);
                outerBorder.Remove(cell);
            }
            else if (cell.PlayerId != PlayerId && cell.GetNeighbours().Any(h => h.PlayerId == PlayerId))
            {
                innerBorder.Remove(cell);
                outerBorder.Add(cell);
            }
            else
            {
                innerBorder.Remove(cell);
                outerBorder.Remove(cell);
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

        private void AssignPopulation()
        {
            var gameData = gameDataProvider.Get();
            population = Mathf.Clamp(population, 0, TotalCellsCount * gameData.MaxDensePopulation);

            if (TotalCellsCount != 0)
            {
                DensePopulation = Population / TotalCellsCount;
            }
            else
            {
                DensePopulation = 0;
            }
        }

        public Modifiers GetModifiers()
        {
            return CustomModifiers ?? Modifiers.Default;
        }
        
        public IEnumerator<Hex> GetEnumerator()
        {
            return cellsSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public class Modifiers
        {
            public static readonly Modifiers Default = new();

            public float IncomeMultiplier { get; set; } = 1;
        }
    }
}