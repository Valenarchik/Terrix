using System.Collections;
using System.Collections.Generic;
using System;

namespace CustomUtilities.DataStructures
{
    public class IndexList<T> : IReadOnlyIndexList<T>
    {
        private int lastIndex;
        private readonly Dictionary<int, T> indexToObject;
        private readonly Dictionary<T, int> objectToIndex;

        public IReadOnlyDictionary<T, int> ObjectToIndex => objectToIndex;
        public IReadOnlyDictionary<int, T> IndexToObject => indexToObject;

        public int Count => indexToObject.Count;
        public IEnumerable<int> Keys => indexToObject.Keys;
        public IEnumerable<T> Values => indexToObject.Values;

        public IndexList()
        {
            lastIndex = 0;
            indexToObject = new Dictionary<int, T>();
            objectToIndex = new Dictionary<T, int>();
        }

        public IndexList(IReadOnlyDictionary<int, T> dict) : this()
        {
            var maxIndex = int.MinValue;
            foreach (var pair in dict)
            {
                indexToObject[pair.Key] = pair.Value;
                objectToIndex[pair.Value] = pair.Key;
                maxIndex = Math.Max(maxIndex, pair.Key);
            }

            lastIndex = maxIndex + 1;
        }

        public int Add(T item)
        {
            while (indexToObject.ContainsKey(lastIndex))
            {
                lastIndex++;
            }
            
            var index = lastIndex;
            Add(index, item);
            lastIndex++;
            return index;
        }

        public void Add(int index, T item)
        {
            if (indexToObject.ContainsKey(index))
            {
                throw new ArgumentException($"Index is already taken! Index = {index} Item = {item}");
            }

            indexToObject[index] = item;
            objectToIndex[item] = index;
        }

        public void Remove(T item)
        {
            if (!objectToIndex.ContainsKey(item))
            {
                return;
            }
            
            var index = objectToIndex[item];
            
            indexToObject.Remove(index);
            objectToIndex.Remove(item);
        }

        public void Remove(int index)
        {
            if (!indexToObject.ContainsKey(index))
            {
                return;
            }

            var item = indexToObject[index];
            
            indexToObject.Remove(index);
            objectToIndex.Remove(item);
        }

        public void Clear()
        {
            lastIndex = 0;
            indexToObject.Clear();
            objectToIndex.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IReadOnlyIndexList<T>: IEnumerable<T>
    {
        public IReadOnlyDictionary<int, T> IndexToObject { get; }
        public IReadOnlyDictionary<T, int> ObjectToIndex { get; }
    }
}