//using SaintsField;
using System;
using UnityEngine;
using Remedy.Schematics;
using Remedy.Framework;
using System.Linq;
using static ScriptableEventBase;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class SchematicScope : ScriptableObjectWithID<SchematicScope>
{
    public GameObject Prefab;

#if UNITY_EDITOR
    public GUID PrefabGUID;
    public GlobalObjectId PrefabID;
    public List<GlobalObjectId> Components = new();
#endif

    [HideInInspector]
    public SchematicGraph Graph;
    public ScriptableEventBase.Input Events = new() { Subscriptions = new ScriptableEventBase[0] };

    [Tooltip("The original ScriptableEvent Arrays paired with their IOBase ID. This is modified by the FlowGraphEditorWindow, " +
        " and referred to when creating a new Schematic Instance.")]
    public SerializableDictionary<int, ScriptableEventBase[]> IOBaseEvents = new();

    [Serializable]
    public class ScriptableEventReference
    {
        public bool Global = false;
        public UnityEngine.Object Target;
        /// <summary>
        /// The IOBase that that this Event Reference was created for. The Event created for this Reference will
        /// replace the Event at the Index of the Event that this Reference was originally created for
        /// </summary>
        public ScriptableEventBase.IOBase IOBase;
        /// <summary>
        /// The Index in the IOBase's IOEvents where the original Event was at.
        /// </summary>
        public int EventIndex;

        [SerializeField]
        private string _originalName;

        [SerializeField]
        private string _currentName;

        public string CurrentName
        {
            get
            {
                return _currentName;
            }
            set
            {
                _currentName = value;
            }
        }

        [SerializeField]
        private string _directoryPath = "";
        /// <summary>
        /// Gets the Directory Path for the Event. This is set when NewEvent is called, and loaded from the Event when the Reference was created
        /// for an existing Event.
        /// </summary>
        public string DirectoryPath
        {
            get
            {
                return _directoryPath;
            }
        }

        public string AssetPath => _assetPath;  

        [SerializeField]
        private int _type = -1;
        public int Type
        {
            get
            {
                return _type;
            }
            set
            {
                ChangeType(value);
                _type = value;
            }
        }
        //public DropdownList<int> GetSETypes() => ScriptableEventBase.SETypeDropdown;
        /// <summary>
        /// The actual Event Asset stored in the Project. This is set/updated when the ScriptableEventReference is Saved (Creating a new
        /// ScriptableEvent Asset and updating references to the new one.)
        /// </summary>
        [SerializeField]
        private string _assetPath = "";
        public ScriptableEventBase EventAsset
        {
            get
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableEventBase>(_assetPath);
                return asset;
            }
        }
        [SerializeField]
        private ScriptableEventBase _workingEvent = null;
        /// <summary>
        /// <para>
        /// (Get) -> <br/> 
        /// The temporary version of the Event currently being worked on. 
        /// This accessor creates a new ScriptableEvent based off of the actual <see cref="_assetPath"/>
        /// ScriptableEvent Asset, if there is one.
        /// </para>
        /// </summary>
        public ScriptableEventBase WorkingEvent
        {
            get
            {
                return _workingEvent;
            }
        }

        [SerializeField]
        int _replaceIndex = -1;

        /// <summary>
        /// Creates a NewEvent for the IOBase. <br/> Typically called either on Adding an item as defined in the UI for Event Containers in
        /// the FlowGraphEditorWindow, or for each IOBase (ScriptableEvent.Input/Output) that is drawn in the FlowGraphEditorWindow that
        /// doesn't already contain an Event. 
        /// </summary>
        /// <param name="iOBase"></param>
        /// <param name="component"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void CreateNewEventAndReference(ScriptableEventBase.IOBase iOBase, UnityEngine.Object target, string path, string name)
        {
            Target = target;
            _directoryPath = path;

            Type type = iOBase.GetType();

            var genArgs = type.GetGenericArguments();

            Type evType = typeof(ScriptableEvent);
            if (genArgs.Length > 0)
                evType = ScriptableEventBase.GetEventTypeForArgumentType(type.GetGenericArguments()[0]);

            Type = ScriptableEventBase.ScriptableEventTypes.IndexOf(evType);
            IOBase = iOBase;

            _workingEvent = (ScriptableEventBase)CreateInstance(evType);
            
            //ChangeType(_type);
            CurrentName = name;

            if(!string.IsNullOrEmpty(name))
                _originalName = name;

            var events = iOBase.IOEvents.ToList();

            EventIndex = events.Count-1;
        }

        /// <summary>
        /// Creates a new ScriptableEventReference from a ScriptableEvent Asset <br/> Typically called on Redraw of the EditorWindow's IODock
        /// for IOBase instances (ScriptableEvent.Input/Output) for each Event that they contain on draw. (if they contain no Events, 
        /// <see cref="CreateNewEventAndReference(ScriptableEventBase.IOBase, Component, string, string)"/> is called.
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="iOBase"></param>
        /// <param name="iOEventIndex"></param>
        public void CreateReferenceFromExistingEvent(ScriptableEventBase ev, UnityEngine.Object target, ScriptableEventBase.IOBase iOBase, int iOEventIndex)
        {
            Target = target;

            var path = AssetDatabase.GetAssetPath(ev);

            if(!string.IsNullOrEmpty(path))
            {
                _workingEvent = Instantiate(ev);
                _workingEvent.name = ev.name;

                _currentName = ev.name;

                var dirPath = Path.GetDirectoryName(path);

                _directoryPath = dirPath.Replace('\\', '/');
                _assetPath = path;

                if (_type == -1)
                    _type = ScriptableEventBase.ScriptableEventTypes.IndexOf(ev.GetType());

                IOBase = iOBase;
                EventIndex = iOEventIndex;

                if (!string.IsNullOrEmpty(ev.name))
                    _originalName = ev.name;
            }
        }

        /// <summary>
        /// Attempts to Change the Event Type to the Type in the <see cref="ScriptableEventBase.ScriptableEventTypes"/> Array at the given Index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void ChangeType(int typeIndex)
        {
            try
            {
                if (WorkingEvent == null) return;
                string name = WorkingEvent.name;
                _workingEvent = (ScriptableEventBase)CreateInstance(ScriptableEventBase.ScriptableEventTypes[typeIndex]);
                WorkingEvent.name = name;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// Invoke this before the Saving Phase so the Asset Database doesn't get confused.
        /// </summary>
        public void DeleteOld()
        {
            var list = IOBase.IOEvents;

            if (list.Contains(EventAsset))
            {
                _replaceIndex = list.IndexOf(EventAsset);
            }

            // A temporary WorkingEvent must exist, and a Directory Path must be defined or found, or this Reference can not be saved.
            if (WorkingEvent == null)
            {
                Debug.LogWarning("WorkingEvent is null. Cannot save.");
                return;
            }
            else if (string.IsNullOrEmpty(DirectoryPath))
            {
                Debug.LogError("DirectoryPath Empty, cannot save: " + WorkingEvent.name);
                return;
            }

            try
            {
                if(EventAsset != null)
                {
                    AssetDatabase.DeleteAsset(AssetPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            try
            {
                string newAssetPath = DirectoryPath + "/" + CurrentName + ".asset";
                var eventAsset = AssetDatabase.LoadAssetAtPath<ScriptableEventBase>(newAssetPath);

                if (eventAsset != null)
                {
                    if (AssetDatabase.LoadAssetAtPath<ScriptableEventBase>(newAssetPath) != null)
                        AssetDatabase.DeleteAsset(newAssetPath);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Attempts to save the <see cref="WorkingEvent"/> at the <see cref="DirectoryPath"/> 
        /// </summary>
        public void Save()
        {
            string newAssetPath = DirectoryPath + "/" + CurrentName + ".asset";
            try
            {
                var evType = ScriptableEventBase.ScriptableEventTypes[Type];
                if (evType != typeof(ScriptableEvent))
                {
                    if (WorkingEvent.GetType() != evType)
                    {
                        Debug.LogError("Event Type incorrect");
                    }
                }

                AssetDatabase.CreateAsset(WorkingEvent, newAssetPath);
                _assetPath = newAssetPath;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Updates the actual IOBase, after it had already been saved.
        /// </summary>
        public void UpdateIOBase()
        {
            if(EventAsset == null)
            {
                Save();
            }

            if (_assetPath != null)
            {
                try
                {
                    _workingEvent = Instantiate(EventAsset);
                    _workingEvent.name = EventAsset.name;
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                var evType = ScriptableEventBase.ScriptableEventTypes[Type];

                _workingEvent = (ScriptableEventBase)CreateInstance(evType);
            }

            var list = IOBase.IOEvents;

            if (_replaceIndex >= 0)
            {
                list[_replaceIndex] = EventAsset;
            }
            else
            {
                if (!list.Contains(EventAsset))
                    list.Add(EventAsset);
            }

            IOBase.IOEvents = list;

        }
#endif
    }
}