// using System;
// using System.Collections.Generic;
// using System.Linq;
// using JetBrains.Annotations;
// using MoreLinq;
// using Terrix.Entities;
// using Terrix.Map;
// using Terrix.Settings;
//
// namespace Terrix.Game.GameRules
// {
//     public interface IAttackInvoker : ITickHandler
//     {
//         public void AddAttack(Attack attack);
//         public void RemoveAttack(Attack attack);
//     }
//
//     public class AttackInvoker : IAttackInvoker
//     {
//         private readonly HexMap map;
//         private readonly IGameDataProvider gameDataProvider;
//         private readonly IPlayersProvider playersProvider;
//         
//         private readonly List<Attack> attacksOrder = new();
//         private readonly Dictionary<int, Attack> attacksMap = new();
//         private readonly Dictionary<Attack, AttackState> attacksStates = new();
//         
//         private readonly Dictionary<Player, VirtualCountry> virtualCountries = new();
//
//         public AttackInvoker(
//             [NotNull] IPlayersProvider playersProvider,
//             [NotNull] HexMap map,
//             [NotNull] IGameDataProvider gameDataProvider)
//         {
//             this.map = map ?? throw new ArgumentNullException(nameof(map));
//             this.gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
//             this.playersProvider = playersProvider ?? throw new ArgumentNullException(nameof(playersProvider));
//             
//             foreach (var player in playersProvider.GetAll())
//             {
//                 virtualCountries[player] = new VirtualCountry(player.Country);
//                 virtualCountries[player].ListenUpdates();
//             }
//         }
//         
//         public void AddAttack(Attack attack)
//         {
//             if (attack is null || attack.Territory.Count == 0)
//             {
//                 return;
//             }
//
//             if (attacksMap.TryAdd(attack.ID, attack))
//             {
//                 attacksOrder.Add(attack);
//                 attacksStates.Add(attack, new AttackState(attack, virtualCountries[attack.Owner]));
//                 InitializeBFS(attacksStates[attack]);
//             }
//         }
//         
//         public void RemoveAttack(Attack attack)
//         {
//             RemoveAttackInternal(attack);
//         }
//         
//         public void HandleTick()
//         {
//             virtualCountries.Values.ForEach(country => country.DontListenUpdates());
//             VirtualAttacks();
//             ApplyVirtualChanges();
//             virtualCountries.Values.ForEach(country => country.ListenUpdates());
//         }
//         
//         private void VirtualAttacks()
//         {
//             foreach (var attack in attacksOrder.ToArray())
//             {
//                 VirtualBFS(attacksStates[attack]);
//             }
//         }
//
//         private void RemoveAttackIfFinished(Attack attack)
//         {
//             var state = attacksStates[attack];
//             if (state.Queue.Count == 0)
//             {
//                 RemoveAttackInternal(attack);
//             }
//         }
//
//         private void RemoveAttackInternal(Attack attack)
//         {
//             if (attacksMap.Remove(attack.ID))
//             {
//                 attacksOrder.Remove(attack);
//                 attacksStates.Remove(attack);
//             }
//         }
//         
//         private void VirtualBFS(AttackState attackState)
//         {
//             while (attackState.Queue.Count > 0 || attackState.Distances[attackState.Queue.Peek()] <= attackState.CurrentDistance)
//             {
//                 var cell = attackState.Queue.Dequeue();
//                 var virtualOwner = GetCellVirtualOwner(cell);
//                 var virtualCost = GetVirtualCellCost(cell, virtualOwner);
//
//                 if (virtualCost > attackState.CurrentPoints)
//                 {
//                     continue;
//                 }
//                 
//                 CaptureVirtualCell(cell, attackState, virtualOwner, virtualCost);
//
//                 foreach (var neighbour in cell.GetNeighbours(map))
//                 {
//                     if (attackState.Distances.ContainsKey(neighbour) // если уже посещал
//                         || attackState.VirtualCountry.VirtualCellsSet.Contains(neighbour) // если часть страны
//                         || !neighbour.GetHexData(gameDataProvider.Get()).CanCapture // если нельзя захватить
//                         || GetCellVirtualOwner(neighbour).PlayerId != attackState.Attack.Owner?.ID) // если не атакуем этого игрока
//                     {
//                         continue;
//                     }
//                     
//                     attackState.Distances[neighbour] = attackState.CurrentDistance + 1;
//                     attackState.Queue.Enqueue(neighbour);
//                 }
//             }
//
//             attackState.CurrentDistance++;
//         }
//         
//         private void InitializeBFS(AttackState attackState)
//         {
//             var startCells = attackState.Attack.Owner.Country.GetOuterBorder();
//             startCells.IntersectWith(attackState.Attack.Territory);
//             
//             foreach (var cell in startCells)
//             {
//                 if (!cell.GetHexData(gameDataProvider.Get()).CanCapture || cell.PlayerId != attackState.Attack.Owner?.ID)
//                 {
//                     continue;
//                 }
//                 
//                 attackState.Distances[cell] = 0;
//                 attackState.Queue.Enqueue(cell);
//             }
//
//             attackState.CurrentDistance = 0;
//         }
//         
//         private void CaptureVirtualCell(Hex captureCell, AttackState attackState, VirtualCountry virtualOwner, float virtualCost)
//         {
//             attackState.CurrentPoints -= virtualCost;
//
//             if (virtualOwner != null)
//             {
//                 virtualOwner.VirtualCellsSet.Remove(captureCell);
//                 attackState.VirtualCountry.VirtualCellsSet.Add(captureCell);
//             }
//         }
//
//         private float GetVirtualCellCost(Hex cell, VirtualCountry owner)
//         {
//             return cell.GetCost(owner?.RealCountry.PlayerId, playersProvider, gameDataProvider.Get());
//         }
//
//         private VirtualCountry GetCellVirtualOwner(Hex cell)
//         {
//             return virtualCountries.Values.FirstOrDefault(c => c.VirtualCellsSet.Contains(cell));
//         }
//         
//         private void ApplyVirtualChanges()
//         {
//             foreach (var (player, virtualCountry) in virtualCountries)
//             {
//                 var country = player.Country;
//                 var removes = country.CellsSet.Except(virtualCountry.VirtualCellsSet).ToArray();
//                 var added = virtualCountry.VirtualCellsSet.Except(country.CellsSet).ToArray();
//
//                 var changeData = new List<Country.CellChangeData>(removes.Length + added.Length);
//                 changeData.AddRange(removes.Select(remove => new Country.CellChangeData(remove, Country.UpdateCellMode.Remove)));
//                 changeData.AddRange(added.Select(add => new Country.CellChangeData(add, Country.UpdateCellMode.Add)));
//                 
//                 player.Country.UpdateCells(new Country.UpdateCellsData(player.ID, changeData.ToArray()));
//             }
//         }
//
//
//         private class AttackState
//         {
//             public Attack Attack { get; set; }
//             public float CurrentPoints { get; set; }
//             public Queue<Hex> Queue { get; set; } = new();
//             public int CurrentDistance { get; set; }
//             public Dictionary<Hex, int> Distances { get; set; } = new();
//             public VirtualCountry VirtualCountry { get; set; }
//
//             public AttackState(Attack attack, VirtualCountry virtualCountry)
//             {
//                 Attack = attack;
//                 CurrentPoints = Attack.Points;
//                 VirtualCountry = virtualCountry;
//             }
//         }
//         
//         private class VirtualCountry
//         {
//             public int PlayerId => RealCountry.PlayerId;
//             public HashSet<Hex> VirtualCellsSet { get; set; } = new();
//             public Country RealCountry { get; set; }
//
//             public VirtualCountry(Country realCountry)
//             {
//                 RealCountry = realCountry;
//             }
//
//             public void ListenUpdates()
//             {
//                 RealCountry.OnCellsUpdate += RealCountryOnCellsUpdate;
//             }
//
//             public void DontListenUpdates()
//             {
//                 RealCountry.OnCellsUpdate -= RealCountryOnCellsUpdate;
//             }
//
//             private void RealCountryOnCellsUpdate(Country.UpdateCellsData data)
//             {
//                 foreach (var updateData in data.ChangeData)
//                 {
//                     var cell = updateData.Hex;
//                     switch (updateData.Mode)
//                     {
//                         case Country.UpdateCellMode.Add:
//                         {
//                             VirtualCellsSet.Add(cell);
//                             break;
//                         }
//                         case Country.UpdateCellMode.Remove:
//                         {
//                             VirtualCellsSet.Remove(cell);
//                             break;
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }