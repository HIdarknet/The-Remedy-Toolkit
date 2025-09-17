using SchematicAssets;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static ScriptableEventBase;

[Serializable]
public class ScriptableEventReference
{
    [SerializeField]
    private string _name;

    public GlobalObjectId EventAssetID;
    public SerializableProperty Property;

    /// <summary>
    /// Called when the Original Event Asset is replaced, so that extra reference update functionality can be invoked for it. 
    /// </summary>
    public Action<GlobalObjectId, GlobalObjectId> EventAssetReplaced;
    /// <summary>
    /// A Property that gets the Event Asset from it's Global ID, casted to ScriptableEventBase
    /// </summary>
    public ScriptableEventBase EventAsset => (ScriptableEventBase)GlobalObjectId.GlobalObjectIdentifierToObjectSlow(EventAssetID);
    public string EventName
    {
        get
        {
            var ev = EventAsset;
            
            if (ev == null)
            {
                Initialize(_defaultType, _defaultName);
                ev = EventAsset;
            }

            return ev.name;
        }
    }

    public Type EventType
    {
        get
        {
            var ev = EventAsset;

            if (ev == null)
            {
                Initialize(_defaultType, _defaultName);
                ev = EventAsset;
            }

            return ev.GetType();
        }
    }

    private ScriptableEventBase _event => (ScriptableEventBase)GlobalObjectId.GlobalObjectIdentifierToObjectSlow(EventAssetID);
    private GameObject _cachedPrefab;
    private GameObject _prefab => _cachedPrefab ??= GetPrefabFromTargetId(Property.ObjID.targetPrefabId);

    [SerializeField]
    private SerializableType _defaultType;
    [SerializeField]
    private string _defaultName;
    [SerializeField]
    private string _fieldName;
    public string FieldName => _fieldName;

    public ScriptableEventReference(GlobalObjectId componentID, string propertyPath, MemberWrapper field, Type defaultType, string defaultName, Action<GlobalObjectId, GlobalObjectId> eventAssetReplacedCallback)
    {
        _name = field.Name;
        _fieldName = field.Name;
        if(string.IsNullOrEmpty(propertyPath))
            Property = new SerializableProperty(componentID, _fieldName);
        else
            Property = new SerializableProperty(componentID, propertyPath + "." + _fieldName);
        
        Initialize(defaultType, defaultName);
        EventAssetReplaced += eventAssetReplacedCallback;
    }

    /// <summary>
    /// Changes the type of the Scriptable Event Asset this References
    /// </summary>
    /// <param name="typeIndex"></param>
    internal void ChangeType(int typeIndex)
    {
        var currentEvent = EventAsset;

        if(currentEvent == null)
        {
            Debug.LogError("Cannot change the Type of a nonexistent Scriptable Event");
            return;
        }    

        string name = currentEvent.name;
        var newEventType = ScriptableEventBase.ScriptableEventTypes[typeIndex];

        CreateEventAsset(newEventType, name); // Creates a new Event since it has a different type than the OG Asset
    }

    /// <summary>
    /// Changes the name of the Scriptable event Asset this References
    /// </summary>
    /// <param name="name"></param>
    internal void ChangeName(string oldName, string newName)
    {
        SchematicAssetManager.Rename(Property.Obj, Property.Path, "", oldName, newName);
    }

    private void Initialize(Type defaultType, string name = "")
    {
        var existing = TryGetExistingEvents();

        if (existing == null || existing.Length == 0)
            CreateEventAsset(defaultType, name); // Create new 
        else
            EventAssetID = GlobalObjectId.GetGlobalObjectIdSlow(existing[0]); // Init from loaded

        UpdateIOBase();
    }

    private ScriptableEventBase[] TryGetExistingEvents()
    {
        var loaded = SchematicAssetManager.LoadAll<ScriptableEventBase>(Property.Obj, Property.Path, "");
        return loaded;
    }

    private void CreateEventAsset(Type eventType, string name = "")
    {
        ScriptableEventBase workingEvent = null;
        try
        {
            if (eventType == null)
                eventType = typeof(ScriptableEvent);

            workingEvent = (ScriptableEventBase)ScriptableObject.CreateInstance(eventType);
            if (string.IsNullOrEmpty(name))
                name = _fieldName;
            workingEvent.name = name;

            var oldID = EventAssetID;

            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(Property.ObjID);
            //SchematicAssetManager.DeleteAll(obj, Property.Path, _fieldName);
            var eventAsset = SchematicAssetManager.Create(obj, Property.Path, "", workingEvent.name, eventType);
            EventAssetID = GlobalObjectId.GetGlobalObjectIdSlow(eventAsset);

            var newID = EventAssetID;

            EventAssetReplaced?.Invoke(oldID, newID);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            if (workingEvent != null)
            {
                UnityEngine.Object.DestroyImmediate(workingEvent);
            }
        }
    }

    private void UpdateIOBase()
    {
        var iobase = Property.GetValue<IOBase>();
        
        if (iobase == null)
        {
            Debug.LogWarning("IOBase is null!");
            return;
        }

        var evList = iobase.IOEvents;
        if (!evList.Contains(_event))
        {
            Undo.RecordObject(Property.Obj, "Add IOEvent");
            evList.Add(_event);
        }
        evList.RemoveAll(item => item == null);
        iobase.IOEvents = evList;
        EditorUtility.SetDirty(Property.Obj);

        AttemptPrefabRefresh();
    }

    private void AttemptPrefabRefresh()
    {
        var prefab = _prefab;
        EditorUtility.SetDirty(prefab);

        PrefabRefreshUtility.ReimportAndResetPrefab(prefab);
    }

    private GameObject GetPrefabFromTargetId(ulong targetPrefabId)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var goId = GlobalObjectId.GetGlobalObjectIdSlow(prefab);
                if (goId.targetPrefabId == targetPrefabId)
                {
                    return prefab;
                }
            }
        }
        return null;
    }
}