using Remedy.Framework;
using System.Collections.Generic;

namespace Remedy.Inventories
{
    public class SyncedInventoryItems : SingletonData<SyncedInventoryItems>
    {
        public List<InventoryItemData> InventoryItems = new();
    }
}