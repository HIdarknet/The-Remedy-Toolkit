using Remedy.Framework;
using UnityEngine;

public class SystemData : SingletonData<SystemData>
{
    [Tooltip("Keys are Component Fields that can be Synced, and Values are whether they are or not.")]
    public SerializableDictionary<string, bool> SyncedProperties;
}