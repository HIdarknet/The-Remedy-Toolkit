using Remedy.Framework;
//using SaintsField.Playa;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Inventories
{
    //[Searchable]
    public class InventoryManager : Singleton<InventoryManager>
    {
        private Dictionary<GameObject, int> _inventories;
        public static Dictionary<GameObject, int> Inventories => Instance._inventories ??= new();
        private Dictionary<GameObject, int> _collectableInventories;
        public static Dictionary<GameObject, int> CollectableInventories => Instance._collectableInventories ??= new();

        public static void AddInventory(Inventory inventory)
        {
            Inventories.Add(inventory.gameObject, inventory.InventoryLayer);
        }
        public static void RemoveInventory(Inventory inventory)
        {
            Inventories.Remove(inventory.gameObject);
        }

        public static void AddCollectableInventory(CollectableInventory collectableInventory)
        {
            Inventories.Add(collectableInventory.gameObject, collectableInventory.CollectionData.InventoryLayer);
        }
        public static void RemoveCollectableInventory(CollectableInventory collectableInventory)
        {
            Inventories.Remove(collectableInventory.gameObject);
        }

        /// <summary>
        /// Gets the Inventory Layer that the GameObject instance is on.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static int GetInventoryLayer(GameObject instance)
        {
            if (Inventories.TryGetValue(instance, out var layer))
            {
                return layer;
            }
            else if (CollectableInventories.TryGetValue(instance, out layer))
            {
                return layer;
            }

            return -1;
        }
    }
}