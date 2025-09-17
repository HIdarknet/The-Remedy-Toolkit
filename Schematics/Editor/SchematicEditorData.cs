using Remedy.Framework;
using Remedy.Schematics;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static ScriptableEventBase;
using SchematicAssets;
using UnityEditor;

public class SchematicEditorData : SingletonData<SchematicEditorData>
{
    [SerializeField]
    internal List<SchematicPrefabData> _prefabData = new();

    [SerializeField]
    internal List<SchematicGraph> Graphs;
    [SerializeField]
    internal List<SchematicScope> Scopes;

    public List<IOBase> IOBases = new();

    public SerializableDictionary<IOBase, List<SchematicScope.ScriptableEventReference>> WorkingEventSetDebug;

    public List<ScriptableEventReference> ScriptableEventReferenceDebug = new();

    /// <summary>
    /// A cache storing the Invoke Nodes of the Schematic with the ScriptableEventReference that their Event is from so they can be properly updated.
    /// </summary>
    public SerializableDictionary<SchematicScope.ScriptableEventReference, List<FlowInvokeBase>> _invokeNodesToReferenceCache = new();
    /// <summary>
    /// A cache storing the OnInvoke Nodes of the Schematic with the ScriptableEventReference that their Event is from so they can be properly updated.
    /// </summary>
    public SerializableDictionary<SchematicScope.ScriptableEventReference, List<FlowOnInvokeBase>> _onInvokeNodesToReferenceCache = new();

    /// <summary>
    /// Adds Schematic Data for the given item
    /// 
    /// This is used in the SchematicGenerationPrefabPostProcessor when asset importing has finished processing, to ensure that Ghost folders
    /// are not retained for Prefabs that no longer exist. 
    /// </summary>
    internal static void AddSchematicData(GameObject prefab)
    {
        var globalId = GlobalObjectId.GetGlobalObjectIdSlow(prefab).ToString();
        var guid = GetPrefabGuid(prefab);

        if(!Instance._prefabData.Any(data => data.GlobalID == globalId))
        {
            Instance._prefabData.Add(new(globalId));
        }
    }

    internal static void DeleteSchematicPrefabs(string[] deleted)
    {
        foreach (var toDelPath in deleted)
        {
            var toDelID = AssetDatabase.GUIDFromAssetPath(toDelPath).ToString();

            SchematicAssetManager.DeleteObjectFolder(toDelID);

            Instance._prefabData.RemoveAll(data => data.GlobalID == toDelID);
        }
    }

    internal static void DeleteComponentData(UnityEngine.GameObject prefab, string componentPath, Type componentType)
    {
        var obj = prefab.transform.Find(componentPath).gameObject.GetComponent(componentType);
        var objID = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
        SchematicAssetManager.DeleteObjectFolder(objID);
    }

    private static string GetPrefabGuid(GameObject prefab)
    {
        if (prefab == null) return null;

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(assetPath))
            return null;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        return guid;
    }

    [Serializable]
    public class SchematicPrefabData
    {
        public string GlobalID;
        public SchematicScope Scope;
        public List<SchematicGraph> Graphs = new();

        public SchematicPrefabData(string globalID)
        {
            GlobalID = globalID;
        }
    }

}