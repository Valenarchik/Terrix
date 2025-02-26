using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CustomUtilities.DataStructures
{
    /// <summary>
    /// Вечный генератор чисел
    /// Пример: start = 1, excludedNums = {2,4,5,10}
    /// Результат: 1,3,6,7,8,9,11,12,13,14...
    /// </summary>
    public class ExclusionarySequence: IEnumerable<int>
    {
        private readonly int start;
        private readonly IEnumerator<int> occupiedPositionsEnumerator;

        private int next;
        private int nextExcluded;

        public ExclusionarySequence(int start, IEnumerable<int> excludedNums)
        {
            this.start = start;
            next = start;
            occupiedPositionsEnumerator = excludedNums
                .Distinct()
                .OrderBy(e => e)
                .GetEnumerator();
            
            FindNextOccupiedPosition();
        }

        private void FindNextOccupiedPosition()
        {
            nextExcluded = occupiedPositionsEnumerator.MoveNext()
                ? occupiedPositionsEnumerator.Current
                : start - 1;
        }

        public int Peek()
        {
            while (true)
            {
                if (next != nextExcluded)
                {
                    return next;
                }

                FindNextOccupiedPosition();
                next++;
            }
        }

        public int Pop()
        {
            Peek();
            return next++;
        }

        public IEnumerator<int> GetEnumerator()
        {
            while (true)
            {
                yield return Pop();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}