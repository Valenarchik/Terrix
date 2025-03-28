using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace CustomUtilities.Extensions
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Без материализации коллекции. Рекомендуется для использования с большими коллекциями.
        /// </summary>
        public static T RandomElementReservoir<T>(this IEnumerable<T> enumerable, Random random = null)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }
            
            random ??= new Random();
            T selected = default;
            var count = 0;
    
            foreach (var element in enumerable)
            {
                count++;
                if (random.Next(count) == 0)
                {
                    selected = element;
                }
            }
    
            if (count == 0)
            {
                throw new InvalidOperationException("Enumerable is empty!");
            }

            return selected;
        }

        /// <summary>
        /// Без материализации коллекции. Рекомендуется для использования с большими коллекциями.
        /// </summary>
        public static T RandomElementReservoirOrDefault<T>(this IEnumerable<T> enumerable, Random random = null)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }
            
            random ??= new Random();
            T selected = default;
            var count = 0;
    
            foreach (var element in enumerable)
            {
                count++;
                if (random.Next(count) == 0)
                {
                    selected = element;
                }
            }

            return selected;
        }
        
        /// <summary>
        /// Находит случайный элемент в enumerable. Внимание! Не подходит для больших enumerable, так как материализирует коллекцию.
        /// </summary>
        public static T RandomElement<T>(this IEnumerable<T> enumerable, Random random = null)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            random ??= new Random();
            
            var list = enumerable as IReadOnlyList<T> ?? enumerable.ToList();
            return list.Count == 0
                ? throw new InvalidOperationException("Cant get random element because enumerable is empty!")
                : list[random.Next(0, list.Count)];
        }
        
        /// <summary>
        /// Находит случайный элемент в enumerable. Внимание! Не подходит для больших enumerable, так как материализирует коллекцию.
        /// </summary>
        public static T RandomElement<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, Random random = null)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return RandomElement(enumerable.Where(predicate), random);
        }

        /// <summary>
        /// Находит случайный элемент в enumerable. Внимание! Не подходит для больших enumerable, так как материализирует коллекцию.
        /// </summary>
        public static T RandomElementOrDefault<T>(this IEnumerable<T> enumerable, Random random = null)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            random ??= new Random();

            var list = enumerable as IReadOnlyList<T> ?? enumerable.ToList();
            return list.Count == 0 ? default : list[random.Next(0, list.Count)];
        }

        /// <summary>
        /// Находит случайный элемент в enumerable. Внимание! Не подходит для больших enumerable, так как материализирует коллекцию.
        /// </summary>
        public static T RandomElementOrDefault<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, Random random = null)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return RandomElementOrDefault(enumerable.Where(predicate), random);
        }

        public static IEnumerable<T> RandomElements<T>(
            this IEnumerable<T> enumerable,
            int elementsCount,
            bool unique = false,
            Random random = null)
        {
            random ??= new Random();
            
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (elementsCount < 0)
            {
                throw new InvalidOperationException($"Arg {nameof(elementsCount)} must be non-negative!");
            }
            
            var list = enumerable.ToList();

            if (unique && list.Count < elementsCount)
            {
                throw new InvalidOperationException($"Arg {nameof(elementsCount)} must be less {nameof(enumerable)} count when unique is true!");
            }

            if (list.Count == 0)
            {
                throw new InvalidOperationException("Cant get random element because enumerable is empty!");
            }

            if (unique)
            {
                for (var i = 0; i < elementsCount; i++)
                {
                    var randomElement = list[random.Next(0, list.Count)];
                    list.Remove(randomElement);
                    yield return randomElement;
                }
            }
            else
            {
                for (var i = 0; i < elementsCount; i++)
                {
                    var randomElement = list[random.Next(0, list.Count)];
                    yield return randomElement;
                }
            }
        }

        public static Stack<T> ToStack<T>(this IEnumerable<T> enumerable, bool useReverse = false)
        {
            var tempList = enumerable.ToList();
            if (useReverse)
            {
                tempList.Reverse();
            }

            var result = new Stack<T>();
            foreach (var element in tempList)
            {
                result.Push(element);
            }

            return result;
        }

        /// <summary>
        /// Берет компонент у каждого элемента коллекции
        /// </summary>
        public static IEnumerable<TComponent> GetComponentMany<TComponent>(this IEnumerable<GameObject> enumerable)
        {
            return enumerable
                .Select(x => x.GetComponent<TComponent>())
                .Where(x => x != null);
        }

        public static T MaxItem<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
        {
            return enumerable.Aggregate((i1, i2) => comparison(i1, i2) > 0 ? i1 : i2);
        }

        public static T MinItem<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
        {
            return enumerable.Aggregate((i1, i2) => comparison(i1, i2) < 0 ? i1 : i2);
        }

        public static void SetActiveAll(this IEnumerable<GameObject> gameObjects, bool value)
        {
            foreach (var gameObject in gameObjects)
            {
                gameObject.SetActive(value);
            }
        }

        public static IEnumerable<GameObject> GameObjects(this IEnumerable<Component> components)
        {
            return components.Select(x => x.gameObject);
        }

        public static IEnumerable<int> GetRandomIntEnumerable(int minValue, int maxValue, Random random = null)
        {
            random ??= new Random();
            
            while (true)
            {
                yield return random.Next(minValue, maxValue);
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is null)
            {
                return true;
            }

            return !enumerable.Any();
        }
        
        public static int GetNextIndexRounded<T>(this IEnumerable<T> enumerable, int currentIndex)
        {
            if (currentIndex == enumerable.Count() - 1)
            {
                return 0;
            }
            return currentIndex + 1;
        }
        
        public static int GetPreviousIndexRounded<T>(this IEnumerable<T> enumerable, int currentIndex)
        {
            if (currentIndex == 0)
            {
                return enumerable.Count() - 1;
            }
            return currentIndex - 1;
        }

        public static IEnumerable<(T1, T2)> Zip<T1, T2>(this IEnumerable<T1> enumerable1, IEnumerable<T2> enumerable2)
        {
            return enumerable1.Zip(enumerable2, (arg1, arg2) => (arg1, arg2));
        }
    }
}