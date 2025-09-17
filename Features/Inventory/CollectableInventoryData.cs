/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;
using Remedy.Common;

namespace Remedy.Inventories
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/Inventory/Collectable Inventory Data")]
    public class CollectableInventoryData : ScriptableObject
    {
        [Tooltip("If true, the Collectable Inventory will render as the Prefab of the first Item in Contents.")]
        public bool RenderAsItem;
        //[Dropdown("GetInventoryLayers")]
        public int InventoryLayer;
        //public DropdownList<int> GetInventoryLayers => GlobalInventoryData.GetInventoryLayers();
        [Tooltip("Items in this Inventory")]
        //[Expandable]
        public InventoryItemCollection Contents;
        [Tooltip("As the name implies, this will be destroyed if it's picked up if True.")]
        public bool DestroyOnPickup = true;
    }
}