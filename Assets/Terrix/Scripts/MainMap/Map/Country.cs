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

        private readonly HashSet<Hex> cellsSet = new();

        public int PlayerId => Owner.ID;
        public float Population { get; private set; }
        public int TotalCellsCount { get; private set; }
        public Player Owner { get; private set; }

        public float DensePopulation => Population / TotalCellsCount;
        public override IReadOnlyCollection<Hex> Cells => cellsSet;
        public event Action<UpdateCellsData> OnCellsUpdate;


        public Country([NotNull] IGameDataProvider gameDataProvider, [NotNull] Player owner) : base(
            Enumerable.Empty<Hex>())
        {
            this.gameDataProvider = gameDataProvider;
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));

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

            foreach (var cellChangeData in changeData)
            {
                cellChangeData.Hex.PlayerId = cellChangeData.Mode switch
                {
                    UpdateCellMode.Add => PlayerId,
                    UpdateCellMode.Remove => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

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
                        }
                        break;
                    }
                    case UpdateCellMode.Remove:
                    {
                        if (cellsSet.Remove(cell))
                        {
                            cellsByTypeCount[cell.HexType]--;
                            TotalCellsCount--;
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }

            OnCellsUpdate?.Invoke(data);
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

            foreach (var changeData in data.ChangeData)
            {
                if (changeData.Mode == UpdateCellMode.Add)
                {
                    if (changeData.Hex.PlayerId != PlayerId)
                    {
                        throw new Exception("Прежде чем добавлять клетки стране, назначте клеткам владельца.");
                    }
                }
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
    }
}