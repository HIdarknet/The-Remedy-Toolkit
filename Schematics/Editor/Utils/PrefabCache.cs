using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PrefabCache
{
    private static Dictionary<ulong, GameObject> _prefabCacheMemory;
    private static Dictionary<ulong, GameObject> _prefabCache
    {
        get
        {
            if (_prefabCacheMemory != null) return _prefabCacheMemory;

            _prefabCache.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    var goId = GlobalObjectId.GetGlobalObjectIdSlow(prefab);
                    _prefabCache[goId.targetPrefabId] = prefab;
                }
            }

            return _prefabCacheMemory;
        }
    }

    private static GameObject GetPrefabFromTargetId(ulong targetPrefabId)
    {
        return _prefabCache.TryGetValue(targetPrefabId, out GameObject prefab) ? prefab : null;
    }

    // Add this to refresh cache when assets change
    [InitializeOnLoadMethod]
    private static void RegisterAssetCallbacks()
    {
        AssetDatabase.importPackageCompleted += _ => RefreshPrefabCache();
        AssetDatabase.importPackageCancelled += _ => RefreshPrefabCache();
    }

    private static void RefreshPrefabCache()
    {
        _prefabCacheMemory = null;
    }
}