using BlueGraph;
using BlueGraph.Editor;
using Remedy.Schematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static ScriptableEventBase;

/// <summary>
/// A custom Unity editor window for editing flow graphs and schematics.
/// Provides a visual interface for creating and managing schematic graphs with drag-and-drop functionality,
/// event management, and real-time validation of flow connections.
/// </summary>
public class SchematicGraphEditorWindow : GraphEditorWindow
{
    /// <summary>
    /// Manages schematic editor events and handles event reference validation and saving.
    /// </summary>
    internal SchematicEditorEventManager EventManager;
    internal string GlobalPath
    {
        get
        {
            var assetPath = AssetDatabase.GetAssetPath(SchematicEditorData.Instance);
            var path = Path.GetDirectoryName(assetPath) + "\\Global Events\\";
            return path;
        }
    }
       
    #region Editing Objects
    /// <summary>
    /// The schematic scope that contains the graph being edited.
    /// This is the main container for the schematic data.
    /// </summary>
    [HideInInspector]
    public SchematicScope SchematicScope;
    /// <summary>
    /// Gets the schematic graph from the current schematic scope.
    /// Returns null if no schematic scope is assigned.
    /// </summary>
    public SchematicGraph SchematicGraph => SchematicScope == null ? null : SchematicScope.Graph;
    /// <summary>
    /// Gets the prefab associated with the current schematic graph.
    /// This prefab represents the GameObject hierarchy that the schematic operates on.
    /// </summary>
    public GameObject Prefab => SchematicGraph.Prefab;
    
    #endregion

    #region Object Properties
    private string _prefabPath;
    /// <summary>
    /// Unique identifier for the current schematic, used for persistent UI state.
    /// </summary>
    internal string SchematicGUID;
    #endregion

    #region UI Elements
    /// <summary>
    /// Main UI container that provides the sidebar and content layout structure.
    /// </summary>
    private SidebarAndContent _sidebarAndContent;
    private TabView _sidebarTabs;
    #endregion

    #region UI Properties
    /// <summary>
    /// A Lookup Table for Types
    /// </summary>
    internal static readonly IReadOnlyDictionary<Type, Color> ColorLookupValueType = new Dictionary<Type, Color>
    {
    { typeof(bool),       new Color(0.9f, 0.3f, 0.3f) }, // red (conditions)
    { typeof(int),        new Color(0.2f, 0.6f, 1f)   }, // blue (whole numbers)
    { typeof(float),      new Color(0.3f, 0.8f, 0.3f) }, // green (continuous values)
    { typeof(string),     new Color(0.8f, 0.5f, 0.8f) }, // purple (text)

    // Unity common types
    { typeof(Vector2),    new Color(1f,   0.7f, 0.2f) }, // orange (2D vectors)
    { typeof(Vector3),    new Color(1f,   0.5f, 0.2f) }, // deeper orange (3D vectors)
    { typeof(Vector4),    new Color(1f,   0.4f, 0.2f) }, // similar but distinct
    { typeof(Quaternion), new Color(0.7f, 0.4f, 1f)   }, // violet (rotations)
    { typeof(RaycastHit), new Color(0.3f, 0.9f, 0.9f) }, // teal (physics result)

    // Unity objects
    { typeof(GameObject), new Color(1f,   1f,  0.4f)  }, // yellow (scene objects)
    { typeof(Transform),  new Color(0.9f, 0.9f, 0.3f) }, // yellow-green
    { typeof(Material),   new Color(0.9f, 0.6f, 0.2f) }, // bronze-like
    { typeof(Texture),    new Color(0.6f, 0.4f, 0.9f) }, // purple-blue

    // Misc
    { typeof(Color),      Color.white                 }, // white (represents itself)
    { typeof(object),     Color.gray                  }  // fallback / unknown
    };

    internal static readonly IReadOnlyDictionary<string, Color> ColorLookupFunctional = new Dictionary<string, Color>()
    {
    { "Remove",             new Color(1, 0, 0, 0.1f)                        },
    { "RemoveHighlight",    Color.darkRed                                   },
    { "Create",             Color.cyan * new Color(0.8f, 0.8f, 0.8f, 0.1f)  },
    { "CreateHighlight",    Color.cyan * new Color(0.8f, 0.8f, 0.8f, 0.25f) },
    };

    internal static readonly IReadOnlyDictionary<int, Dictionary<bool, Color>> ColorLookupHierarchichal = new Dictionary<int, Dictionary<bool, Color>>()
    {
        { // 1st Layer
            0, 
            new() 
            {
                { // %2 == 0
                    false,
                    Color.gray3 * new Color(0.65f,0.65f,0.65f,1f)
                },
                { // %2 == 1
                    true,
                    Color.gray3 * new Color(0.7f,0.7f,0.7f,1f)
                }
            } 
        },

        { // 2nd Layer
            1,
            new()
            {
                { // %2 == 0
                    false,
                    Color.gray3 * new Color(0.9f,0.9f,0.9f,1f)
                },
                { // %2 == 1
                    true,
                    Color.gray3 * new Color(0.85f,0.85f,0.85f,1f)
                }
            }
        }
    };
    #endregion

    #region UI State
    /// <summary>
    /// Determines whether component names should be displayed in the UI.
    /// This setting affects the visibility of component labels in the hierarchy view.
    /// </summary>
    internal bool ShowComponentNames = false;
    /// <summary>
    /// Tracks whether a flash animation is currently in progress to prevent multiple simultaneous animations.
    /// </summary>
    bool flashed = false;
    #endregion

    #region Caches
    /// <summary>
    /// Maps scriptable events to their associated components for quick lookup during node operations.
    /// This cache improves performance when displaying component relationships in the UI.
    /// </summary>
    internal Dictionary<ScriptableEventBase, List<UnityEngine.Object>> TargetEventMap = new();
    /// <summary>
    /// Maps components to their corresponding ObjectField UI elements for efficient UI updates.
    /// </summary>
    private Dictionary<Component, ObjectField> _componentFieldMap = new();
    /// <summary>
    /// Cached list of all invoke nodes in the current graph for performance optimization.
    /// </summary>
    private List<FlowInvokeBase> _cachedInvokeNodes = new();
    /// <summary>
    /// Cached list of all on-invoke nodes in the current graph for performance optimization.
    /// </summary>
    private List<FlowOnInvokeBase> _cachedOnInvokeNodes = new();
    /// <summary>
    /// Static collection of all editor windows that need to be saved.
    /// This ensures proper cleanup and data persistence across multiple editor instances.
    /// </summary>
    private static List<SchematicGraphEditorWindow> _windowsToSave;
    /// <summary>
    /// Gets the list of editor windows that require saving, initializing it if necessary.
    /// </summary>
    public static List<SchematicGraphEditorWindow> WindowsToSave => _windowsToSave ??= new();
    /// <summary>
    /// List of processed IO bases to prevent duplicate processing during graph operations.
    /// </summary>
    private List<IOBase> _processed = new();
    #endregion

    /// <summary>
    /// Loads and initializes a graph in the editor window.
    /// Sets up the UI, event management, caches, and callbacks required for graph editing.
    /// </summary>
    /// <param name="graph">The graph to load and display in the editor</param>
    public override void Load(Graph graph)
    {
        if (Prefab != null)
            SchematicScope.PrefabGUID = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(Prefab));
        else
        {
            var loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(SchematicScope.PrefabGUID));
            if (loadedPrefab != null)
            {
                SchematicScope.Prefab = loadedPrefab;
                SchematicGraph.Prefab = loadedPrefab;
            }
        }

        EventManager = new((SchematicGraph)graph);
        _sidebarAndContent = new SidebarAndContent();
        _sidebarAndContent.SidebarContainer.style.minHeight = Length.Percent(100);

        _sidebarTabs = new()
        {
            style =
            {
                height = Length.Percent(100)
            }
        };
        _sidebarTabs.AddTab("Global");
        _sidebarTabs.AddTab("Prefab");
        _sidebarTabs.AddTab("Variables");

        _sidebarAndContent.SidebarContainer.Add(_sidebarTabs);

        base.Load(graph);

        _sidebarAndContent.contentContainer.Add(Canvas);

        WindowsToSave.Add(this);
        _processed.Clear();

        if (SchematicScope != null)
        {
            var path = AssetDatabase.GetAssetPath(SchematicScope);
            SchematicGUID = AssetDatabase.AssetPathToGUID(path);
            EventManager.WorkingSet.Clear();
            RedrawIODock();
        }
        else
            Debug.LogError("The SchematicScope for this Schematic Prefab does not exist.", this);

        // Graph Canvas
        Canvas.StretchToParentSize();
        Canvas.style.flexGrow = 1;
        Canvas.style.position = Position.Relative;

        rootVisualElement.Add(_sidebarAndContent);

        // Callbacks
        Canvas.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        Canvas.RegisterCallback<DragPerformEvent>(OnDragPerform);

        foreach (var node in Canvas.Query<NodeView>().ToList())
        {
            if (node.Target != null && node.Target is FlowInvokeBase invokeFlowNode)
            {
                _cachedInvokeNodes.Add(invokeFlowNode);

                if (invokeFlowNode.EventBase != null && TargetEventMap.ContainsKey(invokeFlowNode.EventBase))
                {
                    foreach (var component in TargetEventMap[invokeFlowNode.EventBase])
                    {
                        var objectField = new ObjectField()
                        {
                            objectType = typeof(MonoBehaviour),
                            value = component
                        };
                        objectField.SetEnabled(false);

                        node.Add(objectField);
                    }
                }
            }

            if (node.Target != null && node.Target is FlowOnInvokeBase onInvokeFlowNode)
            {
                _cachedOnInvokeNodes.Add(onInvokeFlowNode);

                if (onInvokeFlowNode.EventBase != null && TargetEventMap.ContainsKey(onInvokeFlowNode.EventBase))
                {
                    foreach (var component in TargetEventMap[onInvokeFlowNode.EventBase])
                    {
                        var objectField = new ObjectField()
                        {
                            objectType = typeof(MonoBehaviour),
                            value = component
                        };
                        objectField.SetEnabled(false);
                    }
                }
            }
        }

        if (_prefabPath == null)
            _prefabPath = AssetDatabase.GetAssetPath(Prefab);

        SchematicEditorData.Instance.ScriptableEventReferenceDebug = EventManager.WorkingSet;

        Application.focusChanged += Reload;
        EditorApplication.projectChanged += Canvas.Reload;
        AssemblyReloadEvents.afterAssemblyReload += Canvas.Reload;
    }

    private void OnDisable()
    {
        Application.focusChanged -= Reload;
        EditorApplication.projectChanged -= Canvas.Reload;
        AssemblyReloadEvents.afterAssemblyReload -= Canvas.Reload;
    }

    private void Reload(bool val = false)
    {
        Canvas.Reload();
    }

    /// <summary>
    /// Redraws the Input/Output Event dock in the sidebar.
    /// Refreshes the prefab hierarchy display, component toggles, and schematic foldouts.
    /// This method is called whenever the UI needs to be updated to reflect changes in the graph or prefab.
    /// </summary>
    internal void RedrawIODock()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
        if (prefabAsset != null)
        {
            SchematicGraph.Prefab = prefabAsset;
        }

        SchematicScope.Prefab = Prefab;

        RedrawGlobalIO();
        RedrawPrefabIO();
        RedrawGraphIO();
    }

    private void RedrawGlobalIO()
    {
        _sidebarTabs["Global"].Clear();

        var instProps = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttribute<SchematicGlobalObjectAttribute>() != null)
            .Select(type => type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Select(prop => prop.GetValue(prop.GetType()))
            .OfType<ScriptableObject>();

        foreach (var instance in instProps)
        {
            var attr = instance.GetType().GetCustomAttribute<SchematicGlobalObjectAttribute>();
            var displayName = attr.Name;

            var objFoldout = new PersistentFoldout(SchematicGUID + "_Global_", displayName)
            {
                text = displayName
            };


            var path = AssetDatabase.GetAssetPath(SchematicEditorData.Instance) + "/Global Events/" + displayName + "/";
            (bool draw, VisualElement dock) = IODockRegistry.GetComponentRenderer(instance.GetType()).Render(this, instance, null, path);

            if(draw)
            {
                objFoldout.Add(dock);
                _sidebarTabs["Global"].Add(objFoldout);
            }
        }
    }

    private void RedrawPrefabIO()
    {
        // Component Name Toggle for Component Tabs
        var componentNamesToggle = new Toggle
        {
            text = "Show Component Names",
            value = ShowComponentNames
        };
        componentNamesToggle.RegisterValueChangedCallback(evt =>
        {
            ShowComponentNames = evt.newValue;
            RedrawPrefabIO();
        });

        // Draw
        _sidebarTabs["Prefab"].Clear();
        _sidebarTabs["Prefab"].Add(componentNamesToggle);

        if (Prefab != null)
        {
            _sidebarTabs["Prefab"].Add(new PrefabHierarchy(Prefab, this, true, true));
        }
    }

    private void RedrawGraphIO()
    {
        // Draw
        _sidebarTabs["Variables"].Clear();
        _sidebarTabs["Variables"].Add(IODockRegistry.RenderMultipleFields(this, Prefab.GetComponent<SchematicInstanceController>(), Prefab.GetComponent<SchematicInstanceController>()).Item2);
    }

    /// <summary>
    /// Handles drag-and-drop update events over the canvas.
    /// Sets the appropriate visual feedback when objects are being dragged over the canvas.
    /// </summary>
    /// <param name="evt">The drag update event data</param>
    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        if (DragAndDrop.objectReferences.Length > 0)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy; 
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// Handles drag-and-drop perform events when objects are dropped onto the canvas.
    /// Creates appropriate nodes or shows context menus based on the type of dropped object.
    /// </summary>
    /// <param name="evt">The drag perform event data</param>
    private void OnDragPerform(DragPerformEvent evt)
    {
        if (DragAndDrop.objectReferences.Length > 0)
        {
            Vector2 dropPosition = evt.mousePosition;

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is ScriptableEventBase scriptableEvent)
                {
                    ShowEventDropMenu(scriptableEvent, dropPosition);
                }
                else if(obj is ScriptableVariable scriptableVariable)
                {
                    // ShowVariableDropMenu(scriptableVariable, dropPosition);
                }
            }

            DragAndDrop.AcceptDrag();
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// Displays the Event Drop Menu if it was an Event that was dropped as
    /// described in <see cref="OnDragPerform(DragPerformEvent)"/>
    /// enabling creation of Event Nodes in the Graph by the user..
    /// </summary>
    /// <param name="evt">The drag perform event data</param>
    private void ShowEventDropMenu(ScriptableEventBase evtAsset, Vector2 position)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent($"Action ► Invoke {evtAsset.name}"), false, () => {
            CreateInvokeNode(evtAsset, position);
        });

        menu.AddItem(new GUIContent($"Event ► On {evtAsset.name} Invoked"), false, () => {
            CreateOnInvokeNode(evtAsset, position);
        });

        menu.ShowAsContext();
    }

    /// <summary>
    /// Displays a context menu when a ScriptableVariable is dropped onto the canvas.
    /// Currently commented out but reserved for future variable node functionality.
    /// </summary>
    /// <param name="varAsset">The scriptable variable asset that was dropped</param>
    /// <param name="position">The position where the object was dropped</param>
    private void ShowVariableDropMenu(ScriptableVariable varAsset, Vector2 position)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent($"Action ► Set Variable {varAsset.name}"), false, () => {
            //CreateInvokeNode(evtAsset, position);
        });

        menu.AddItem(new GUIContent($"Query ► Get {varAsset.name}"), false, () => {
            //CreateOnInvokeNode(evtAsset, position);
        });

        menu.ShowAsContext();
    }

    /// <summary>
    /// Creates an invoke node for a given scriptable event at the specified position, for drag-and-drop of I/O from the User
    /// </summary>
    /// <param name="evtAsset">The scriptable event to create an invoke node for</param>
    /// <param name="position">The position to place the new node</param>
    private void CreateInvokeNode(ScriptableEventBase evtAsset, Vector2 position)
    {
        foreach(var type in FlowInvokeBase.NodeTypes)
        {
            if(type.BaseType.GetGenericArguments().Length > 1 && evtAsset.GetType().BaseType.GetGenericArguments().Length > 0)
            {
                if (type.BaseType.GetGenericArguments()[1] == evtAsset.GetType().BaseType.GetGenericArguments()[0])
                {
                    var node = (FlowInvokeBase)NodeReflection.GetNodeTypes()[type.Name].CreateInstance();
                    node.EventBase = evtAsset;
                    node.EventID = GlobalObjectId.GetGlobalObjectIdSlow(evtAsset);
                    Canvas.AddNodeFromSearch(node, position , null);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Creates an on-invoke node for a given scriptable event at the specified position, for drag-and-drop of I/O from the User
    /// </summary>
    /// <param name="evtAsset">The scriptable event to create an on-invoke node for</param>
    /// <param name="position">The position to place the new node</param>
    private void CreateOnInvokeNode(ScriptableEventBase evtAsset, Vector2 position)
    {
        foreach (var type in FlowOnInvokeBase.NodeTypes)
        {
            if (type.BaseType.GetGenericArguments().Length > 1 && evtAsset.GetType().BaseType.GetGenericArguments().Length > 0)
            {
                if (type.BaseType.GetGenericArguments()[1] == evtAsset.GetType().BaseType.GetGenericArguments()[0])
                {
                    var node = (FlowOnInvokeBase)NodeReflection.GetNodeTypes()[type.Name].CreateInstance();
                    node.EventBase = evtAsset;
                    node.EventID = GlobalObjectId.GetGlobalObjectIdSlow(evtAsset);
                    Canvas.AddNodeFromSearch(node, position , null);
                    return;
                }
            }
        }
    }


    /// <summary>
    /// Scrolls to and highlights a specific component in the sidebar hierarchy that is focused on based on an interaction from the user.
    /// </summary>
    /// <param name="component">The component to scroll to and highlight</param>
    internal void ScrollToComponent(Component component)
    {
        if (!_componentFieldMap.TryGetValue(component, out var field)) return;

        // Open all parent foldouts
        var parent = field.parent;
        while (parent != null)
        {
            if (parent is Foldout foldout)
            {
                foldout.value = true;

                if (!flashed)
                {
                    var scrollView = rootVisualElement.Q<ScrollView>();

                    var worldPos = field.worldBound.yMin;
                    var containerPos = scrollView.worldBound.yMin;

                    var offsetY = worldPos - containerPos;

                    _ = FlashFoldout(scrollView, foldout, offsetY);
                    flashed = true;
                }
            }
            parent = parent.parent;
        }
    }

    /// <summary>
    /// Highlights a foldout with a flash effect and scrolls it into view.
    /// </summary>
    /// <param name="scrollView">The scroll view used to bring the target into view.</param>
    /// <param name="target">The foldout element to highlight.</param>
    /// <param name="position">The vertical scroll offset to center on the foldout.</param>
    /// <returns>A task that completes when the flash animation finishes.</returns>
    private async Task FlashFoldout(ScrollView scrollView, Foldout target, float position)
    {
        var defaultBackground = target.style.backgroundColor;
        var startPosition = scrollView.scrollOffset.y;

        for (float i = 0; i < 1f; i += 0.05f)
        {
            target.style.backgroundColor = Color.Lerp(Color.darkCyan, defaultBackground.value, i);
            scrollView.scrollOffset = new Vector2(0, Mathf.Lerp(startPosition, position, i));

            await Task.Delay(1);
        }

        target.style.backgroundColor = defaultBackground;
        flashed = false;
    }
}