using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using Terrix.DTO;
using Terrix.Game.GameRules;
using Terrix.Map;
using UnityEngine;

namespace Terrix.Controllers
{
    public class BorderStretcher: MonoBehaviour
    {
        [SerializeField] private float alpha = 1.2f;
        [SerializeField] private float beta = 10f;
        
        public Hex[] StretchBorders(
            Vector3Int startPos,
            Vector3Int endPos,
            HexMap map,
            Country country,
            out int? attackTarget,
            out float attackPoints)
        {
            var start = map[startPos];
            var end = map[endPos];
            
            var direction = (end.WorldPosition - start.WorldPosition).normalized;
            var result = new List<Hex>();
            var visited = new HashSet<Hex>();
            var priorityQueue = new SimplePriorityQueue<Hex, float>();

            var seed = start.GetNeighbours()
                .Where(neighbour => !country.Contains(neighbour) || !neighbour.GetHexData().CanCapture)
                .Select(hex => new {Hex = hex, Direction = (hex.WorldPosition - start.WorldPosition).normalized})
                .OrderByDescending(hex => Vector3.Dot(hex.Direction, direction))
                .Select(hex=> hex.Hex)
                .First();

            attackTarget = seed.PlayerId;
            
            priorityQueue.Enqueue(seed, 0);
        
            var remainingPoints = country.Population;
            var targetReached = false;
        
            while (priorityQueue.Count > 0 && remainingPoints > 0 && !targetReached)
            {
                priorityQueue.TryDequeue(out var cell);
                if (visited.Contains(cell) || country.Contains(cell))
                {
                    continue;
                }
        
                var cellCost = cell.GetCost();
                if (cellCost > remainingPoints)
                {
                    continue;
                }
        
                result.Add(cell);
        
                if (end.Equals(cell))
                {
                    targetReached = true;
                }
                
                remainingPoints -= cellCost;
                visited.Add(cell);
                
                
                foreach (var neighbour in cell.GetNeighbours())
                {
                    if (country.Contains(neighbour) 
                        || visited.Contains(neighbour) 
                        || !neighbour.GetHexData().CanCapture
                        || attackTarget != neighbour.PlayerId)
                    {
                        continue;
                    }
        
                    var priority = CalculatePriority(neighbour);
                    priorityQueue.Enqueue(neighbour, -priority);
                }
            }

            attackPoints = country.Population - remainingPoints;
            return result.ToArray();
            
            float CalculatePriority(Hex current)
            {
                var delta = current.WorldPosition - start.WorldPosition;
            
                var projection = Vector3.Dot(delta.normalized, direction);
            
                var distance = delta.magnitude;
                var distanceFactor = 1f / (1f + distance);
        
                return projection * alpha + distanceFactor * beta;
            }
        }
    }
}