using System;
using System.Collections.Generic;
using System.Linq;

namespace Remedy.Framework
{
    /// <summary>
    /// Provides extension methods for the List class.
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        /// Transfers items from one list to another.
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="receiver">The receiver list.</param>
        /// <param name="items">The items to transfer.</param>
        public static void TransferTo<T>(this List<T> source, List<T> receiver, List<T> items)
        {
            foreach (var item in items)
            {
                receiver.Add(item);
                source.Remove(item);
            }
        }

        /// <summary>
        /// Transfers items from one list to another based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="receiver">The receiver list.</param>
        /// <param name="predicate">The predicate to filter the items.</param>
        public static void TransferTo<T>(this List<T> source, List<T> receiver, Predicate<T> predicate)
        {
            var items = source.Where(x => predicate(x)).ToList();
            foreach (var item in items)
            {
                receiver.Add(item);
                source.Remove(item);
            }
        }

        /// <summary>
        /// Transfers a single item from one list to another.
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="receiver">The receiver list.</param>
        /// <param name="item">The item to transfer.</param>
        public static void TransferTo<T>(this List<T> source, List<T> receiver, T item)
        {
            receiver.Add(item);
            source.Remove(item);
        }

        /// <summary>
        /// Transfers an item from one list to another by index.
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="receiver">The receiver list.</param>
        /// <param name="index">The index of the item to transfer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        public static void TransferTo<T>(this List<T> source, List<T> receiver, int index)
        {
            var item = source.ElementAt(index);
            receiver.Add(item);
            source.Remove(item);
        }

        /// <summary>
        /// Transfers a specified number of items from one list to another by index.
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="receiver">The receiver list.</param>
        /// <param name="index">The starting index of the items to transfer.</param>
        /// <param name="amount">The number of items to transfer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index or the amount is out of range.</exception>
        public static void TransferTo<T>(this List<T> source, List<T> receiver, int index, int amount)
        {
            while (source.Any() && amount > 0)
            {
                var item = source.ElementAt(index);
                receiver.Add(item);
                source.Remove(item);
                amount--;
            }
        }

        /// <summary>
        /// Clones a list by creating a new list with the same items.
        /// </summary>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <param name="source">The source list.</param>
        /// <returns>A new list with the same items as the source list.</returns>
        public static List<T> Clone<T>(this List<T> source)
        {
            return new List<T>(source);
        }

        public static List<(TKey, TValue)> ToTupleList<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            List<(TKey, TValue)> tuples = new List<(TKey, TValue)>();
            foreach (var kvp in dictionary)
            {
                tuples.Add((kvp.Key, kvp.Value));
            }
            return tuples;
        }

        public static List<(TKey, TValue)> ToTupleList<TKey, TValue>(this SerializableDictionary<TKey, TValue> dictionary)
        {
            List<(TKey, TValue)> tuples = new List<(TKey, TValue)>();
            foreach (var key in dictionary.Keys)
            {
                tuples.Add((key, dictionary[key]));
            }
            return tuples;
        }

        /// <summary>
        /// Inserts the Value into the List if the given Index doesn't exist in the List yet, or replaces the value at that Index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public static void ReplaceOrInsert<T>(this List<T> list, int index, T value)
        {
            if (list.Count <= index) list.Insert(index, value);
            list[index] = value;
        }

        public static int IndexOf<T>(this IEnumerable<T> list, Predicate<T> predicate)
        {
            for(int i = 0; i < list.Count(); i++)
            {
                if (list.ElementAt(i).Equals(predicate)) return i;
            }
            return -1;
        }
    }
}