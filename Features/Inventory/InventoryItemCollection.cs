using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Inventories
{
    [Serializable]
    public class InventoryItemCollection : IEnumerable<InventoryItemData>
    {
        public InventoryItemData this[int i]
        {
            get
            {
                return _items[i];
            }
        }

        [SerializeField]
        private List<InventoryItemData> _items = new();
        private static List<InventoryItemData> _tempList = new();

        public int Count => _items.Count;

        public InventoryItemCollection Add(InventoryItemData item)
        {
            if (item != null)
                _items.Add(item);
            return this;
        }

        public InventoryItemCollection AddMultiple(InventoryItemData item, int count)
        {
            if (item == null || count <= 0) return this;

            for (int i = 0; i < count; i++)
                _items.Add(UnityEngine.Object.Instantiate(item));

            return this;
        }

        public InventoryItemCollection AddMultiple<T>(Func<T, bool> predicate, int count) where T : InventoryItemData
        {
            if (predicate == null || count <= 0) return this;

            _tempList.Clear();
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is T typed && predicate(typed))
                    _tempList.Add(_items[i]);
            }

            for (int i = 0; i < count && i < _tempList.Count; i++)
            {
                _items.Add(UnityEngine.Object.Instantiate(_tempList[i]));
            }

            return this;
        }

        public InventoryItemCollection Remove(InventoryItemData item)
        {
            _items.Remove(item);
            return this;
        }

        public InventoryItemCollection Clear()
        {
            _items.Clear();
            return this;
        }

        public bool HasItem<T>(Func<T, bool> predicate = null) where T : InventoryItemData
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is T typedItem)
                {
                    if (predicate == null || predicate(typedItem))
                        return true;
                }
            }
            return false;
        }

        public int CountItems<T>(Func<T, bool> predicate = null) where T : InventoryItemData
        {
            int count = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is T typedItem)
                {
                    if (predicate == null || predicate(typedItem))
                        count++;
                }
            }
            return count;
        }

        public InventoryItemCollection RemoveItems<T>(Func<T, bool> predicate) where T : InventoryItemData
        {
            if (predicate == null) return this;

            _tempList.Clear();
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is T typedItem && predicate(typedItem))
                    _tempList.Add(_items[i]);
            }

            for (int i = 0; i < _tempList.Count; i++)
            {
                _items.Remove(_tempList[i]);
            }

            return this;
        }

        public InventoryItemCollection RemoveItems<T>(Func<T, bool> predicate, int count) where T : InventoryItemData
        {
            if (predicate == null || count <= 0) return this;

            _tempList.Clear();

            for (int i = 0; i < _items.Count && _tempList.Count < count; i++)
            {
                if (_items[i] is T typedItem && predicate(typedItem))
                    _tempList.Add(_items[i]);
            }

            for (int i = 0; i < _tempList.Count; i++)
            {
                _items.Remove(_tempList[i]);
            }

            return this;
        }

        public InventoryItemCollection OrderBy<T>(Comparison<T> comparison) where T : InventoryItemData
        {
            if (comparison == null) return this;

            _tempList.Clear();

            // Extract and cast
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is T typed)
                    _tempList.Add(_items[i]);
            }

            // Sort typed items
            _tempList.Sort((a, b) => comparison((T)a, (T)b));

            // Remove and re-insert
            for (int i = 0; i < _tempList.Count; i++)
                _items.Remove(_tempList[i]);

            for (int i = 0; i < _tempList.Count; i++)
                _items.Add(_tempList[i]);

            return this;
        }

        public InventoryItemCollection TransferFrom(InventoryItemCollection other)
        {
            if (other == null || other._items.Count == 0)
                return this;

            _tempList.Clear();

            for (int i = 0; i < other._items.Count; i++)
            {
                var item = other._items[i];
                _items.Add(item);
                _tempList.Add(item);
            }

            for (int i = 0; i < _tempList.Count; i++)
            {
                other._items.Remove(_tempList[i]);
            }

            return this;
        }

        public IEnumerator<InventoryItemData> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        public static implicit operator List<InventoryItemData>(InventoryItemCollection collection)
        {
            return collection._items;
        }
    }

}