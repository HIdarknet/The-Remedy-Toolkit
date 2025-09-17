using Remedy.Framework;
//using SaintsField;
using UnityEngine;

public class GlobalInventoryData : SingletonData<GlobalInventoryData>
{
    public float ItemPickupRange = 1f;

    [Tooltip("The layers for the Inventory System to determine whether an Inventory can hold an Item.")]
    public string[] InventoryLayers;
/*
    public static DropdownList<int> GetInventoryLayers()
    {
        var list = new DropdownList<int>();

        for (int i = 0; i < GlobalInventoryData.Instance.InventoryLayers.Length; i++)
        {
            list.Add(GlobalInventoryData.Instance.InventoryLayers[i], i);
        }

        return list;
    }*/
}