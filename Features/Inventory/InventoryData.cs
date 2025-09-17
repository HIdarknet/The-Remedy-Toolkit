/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;
using Remedy.Common;

namespace Remedy.Inventories
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/Inventory/Default Inventory")]
    public class InventoryData : ScriptableObject
    {
        [Tooltip("Whether the last Item Picked up should be selected as the Active Item.")]
        public bool SwitchOnPickup = true;
        [Tooltip("Can Collect Inventory Items on this Layer only.")]
        //[Dropdown("GetInventoryLayers")]
        public int InventoryLayer;
        //public DropdownList<int> GetInventoryLayers => GlobalInventoryData.GetInventoryLayers();
        public InventoryItemCollection Contents = new();
    }

}
