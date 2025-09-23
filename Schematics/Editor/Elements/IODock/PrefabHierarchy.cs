using Remedy.Framework;
using Remedy.Schematics;
using SchematicAssets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A custom Unity UI Toolkit element that displays a prefab's GameObjects and components in a foldout hierarchy 
/// with interactive input/output docking for visual programming.
/// </summary>
/// <remarks>
/// Designed for use in editor extensions like <see cref="SchematicGraphEditorWindow"/>. 
/// Provides prefab visualization with component tabs, docking interfaces, and undo/redo support.<br/>
/// <br/>
/// Features:<br/>
/// - Foldout hierarchy with persistent state<br/>
/// - Component tabs with add/remove support<br/>
/// - I/O docking via <see cref="IODockRegistry"/><br/>
/// - Selection synchronization with Unity's inspector<br/>
/// </remarks>
public class PrefabHierarchy : VisualElement
{
    private GameObject _prefab;
    private string _assetPath;
    SerializableDictionary<Transform, SerializableDictionary<SerializableType, VisualElement>> evMap = new ();

    /// <summary>
    /// Creates a new prefab hierarchy view.
    /// </summary>
    /// <param name="prefab">Prefab root GameObject to display.</param>
    /// <param name="editorWindow">Editor window hosting the view, used for state and selection handling.</param>
    /// <param name="drawIO">If true, enables input/output docking UI; otherwise shows components only.</param>
    /// <param name="includeSelf">If true, includes the prefab root in the hierarchy view.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="prefab"/> or <paramref name="editorWindow"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="prefab"/> is not a valid prefab instance.</exception>
    public PrefabHierarchy(GameObject prefab, EditorWindow editorWindow, bool drawIO, bool includeSelf)
    {
        _prefab = prefab;
        _assetPath = AssetDatabase.GetAssetPath(prefab);

        var container = new VisualElement();

        SchematicGraphEditorWindow schematicWindow = null;

        if (editorWindow is SchematicGraphEditorWindow asSchematicWindow)
        {
            schematicWindow = asSchematicWindow;
        }

        if (schematicWindow == null) drawIO = false;

        //schematicWindow.SchematicScope.PrefabID = GlobalObjectId.GetGlobalObjectIdSlow(prefab);

        var rootFoldout = new PersistentFoldout(schematicWindow == null ? editorWindow.name : schematicWindow.SchematicGUID, "Components") { text = "Prefab Component Input/Output", value = true };
        container.Add(rootFoldout);

        rebuildHierarchy();

        void rebuildHierarchy()
        {
            var foldoutMap = new Dictionary<Transform, GameObjectFoldout>();
            var componentMap = new Dictionary<Transform, List<Component>>();

            var components = prefab.GetComponentsInChildren<Component>(true);

            rootFoldout.Clear();

            // Group components by their GameObject (Transform)
            foreach (var component in components.Where(comp => comp.GetType() != typeof(SchematicInstanceController)))
            {
                bool isSelected = Selection.activeGameObject == component.gameObject;

                if (!componentMap.ContainsKey(component.transform))
                    componentMap[component.transform] = new List<Component>();

                componentMap[component.transform].Add(component);

                if (schematicWindow != null)
                {
                    Selection.selectionChanged += () =>
                    {
                        foreach (var obj in Selection.gameObjects)
                        {
                            if (obj.GetComponents<MonoBehaviour>().Contains(component))
                            {
                                schematicWindow.ScrollToComponent(component);
                            }
                        }
                    };
                }
            }

            bool itemStep = false;

            // Step 2: Build foldouts only for valid Transforms
            foreach (var tf in prefab.GetComponentsInChildren<Transform>(true).Where(transform => transform != null && (transform != prefab.transform || includeSelf)))
            {
                var goFoldout = new GameObjectFoldout(tf.gameObject, schematicWindow == null ? editorWindow.name : schematicWindow.SchematicGUID + prefab.transform.GetRelativePath(tf), "Child " + tf.name, tf.name)
                {
                    style =
                    {
                        borderLeftWidth = 1,
                        borderTopWidth = 1,
                        borderLeftColor = Color.gray1 + new Color(0.05f, 0.05f, 0.05f),
                        borderBottomColor = Color.gray1 + new Color(0.05f, 0.05f, 0.05f),
                        borderTopColor = Color.gray1 + new Color(0.05f, 0.05f, 0.05f),
                        borderRightColor = Color.gray1 + new Color(0.05f, 0.05f, 0.05f),
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                };

                goFoldout.OnRenameRequested += (string name) =>
                {
                    var tfPath = prefab.transform.GetRelativePath(tf);
                    var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                    var tfCopy = copy.transform.Find(tfPath);
                    if (tfCopy != null)
                    {
                        tfCopy.name = name;

                        ApplyChangesToPrefabAsset(copy, goFoldout);
                    }
                };

                goFoldout.Q(className: "unity-foldout__toggle").style.marginLeft = 0;
                goFoldout.Q(className: "unity-foldout__toggle").style.marginRight = 0;

                foldoutMap[tf] = goFoldout;

                itemStep = !itemStep;
                goFoldout.style.backgroundColor = itemStep
                    ? Color.gray2
                    : Color.gray2 - new Color(0.02f, 0.02f, 0.02f, 0.1f);

                // === Add Child Button ===
                var addChildButton = new Button(() =>
                {
                    var tfPath = prefab.transform.GetRelativePath(tf);
                    var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                    var child = new GameObject("New Child");

                    var tfCopy = copy.transform.Find(tfPath);
                    if (tfCopy != null)
                    {
                        child.transform.SetParent(tfCopy, false);

                        ApplyChangesToPrefabAsset(copy, goFoldout);
                    }
                })
                {
                    text = "+"
                };
                addChildButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Create"];

                addChildButton.RegisterCallback<MouseEnterEvent>(evt => addChildButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["CreateHighlight"]);
                addChildButton.RegisterCallback<MouseLeaveEvent>(evt => addChildButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Create"]);

                addChildButton.style.position = Position.Absolute;
                addChildButton.style.top = 0;      
                addChildButton.style.right = 0;    
                addChildButton.style.width = 20;
                addChildButton.style.height = 20;

                // === Remove This Button ===
                var removeThisButton = new Button(() =>
                {
                    var tfPath = prefab.transform.GetRelativePath(tf);
                    var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                    var tfCopy = copy.transform.Find(tfPath);
                    if (tfCopy != null)
                    {
                        GameObject.DestroyImmediate(tfCopy.gameObject);

                        ApplyChangesToPrefabAsset(copy, goFoldout);
                    }
                })
                {
                    text = "-"
                };
                removeThisButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"];

                removeThisButton.RegisterCallback<MouseEnterEvent>(evt => removeThisButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["RemoveHighlight"]);
                removeThisButton.RegisterCallback<MouseLeaveEvent>(evt => removeThisButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"]);

                removeThisButton.style.position = Position.Absolute;
                removeThisButton.style.top = 0;
                removeThisButton.style.right = 20;
                removeThisButton.style.width = 20;
                removeThisButton.style.height = 20;

                if (tf.parent != null && foldoutMap.TryGetValue(tf.parent, out var parentFoldout))
                    parentFoldout.Add(goFoldout);
                else
                    rootFoldout.Add(goFoldout);

                goFoldout.Add(addChildButton);
                goFoldout.Add(removeThisButton);
            }

            // Add components and docks after the hierarchy is built
            foreach (var kvp in componentMap)
            {
                var tf = kvp.Key;
                var componentsOnObject = kvp.Value;

                if (!foldoutMap.TryGetValue(tf, out var goFoldout)) continue;

                var foldout = goFoldout.Foldout;

                var ioGroup = new VisualElement();

                ioGroup.style.paddingBottom = 4;
                ioGroup.style.paddingLeft = 6;
                ioGroup.style.paddingRight = 6;
                ioGroup.style.backgroundColor = Color.gray2 - new Color(0.05f, 0.05f, 0.05f, 0.1f);
                ioGroup.style.borderTopWidth = 1;
                ioGroup.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                ioGroup.style.borderBottomWidth = 1;
                ioGroup.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                ioGroup.style.borderLeftWidth = 1;
                ioGroup.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                ioGroup.style.borderRightWidth = 1;
                ioGroup.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

                var componentLabel = new Label("Components");
                componentLabel.style.color = Color.gray5;

                VisualElement tabBar = new VisualElement();
                tabBar.style.flexDirection = FlexDirection.Row;
                tabBar.style.marginTop = 0;
                tabBar.style.marginBottom = 0;

                var tabScrollBar = new FlippedHorizontalScrollView();
                tabScrollBar.Add(tabBar);

                VisualElement tabContentContainer = new VisualElement()
                {
                    name = "Tab Content Container"
                };
                tabContentContainer.style.flexDirection = FlexDirection.Column;

                string selectedTab = null;
                var tabContentMap = new Dictionary<string, VisualElement>();

                int compCount = 0;
                if (true)
                {
                    foreach (var component in componentsOnObject)
                    {
                        var type = component.GetType();
                        var typeName = ObjectNames.NicifyVariableName(type.Name);
                        var icon = EditorGUIUtility.ObjectContent(null, type).image;

                        schematicWindow.SchematicScope.Components.Add(GlobalObjectId.GetGlobalObjectIdSlow(prefab));

                        // Create content container for this tab
                        var content = new VisualElement { name = typeName };
                        content.style.display = DisplayStyle.None;
                        content.style.flexDirection = FlexDirection.Column;
                        content.style.backgroundColor = Color.gray1;

                        if (evMap.ContainsKey(tf) && evMap[tf].ContainsKey(type))
                        {
                            content.Add(evMap[tf][type]);
                        }
                        else
                        {
                            var (draw, dock) = IODockRegistry.GetComponentRenderer(type).Render(schematicWindow, component, prefab.transform);
                            EventContainerRenderer.DrawDelayedEvents();

                            var propertiesField = type.GetFields().Where(field => typeof(ScriptableObject).IsAssignableFrom(field.FieldType) && field.GetCustomAttributes()
                                                                                                            .Any(attr => attr.GetType() == typeof(SchematicPropertiesAttribute))).FirstOrDefault();

                            if ((draw || propertiesField != null))
                            {
                                if (dock == null)
                                    dock = new VisualElement();

                                dock.style.display = DisplayStyle.Flex;
                                dock.style.flexDirection = FlexDirection.Column;
                                dock.style.borderBottomWidth = 1;
                                dock.style.borderTopWidth = 1;
                                dock.style.borderLeftWidth = 1;
                                dock.style.borderRightWidth = 1;
                                dock.style.borderBottomColor = Color.gray4;
                                dock.style.borderTopColor = Color.gray4;
                                dock.style.borderLeftColor = Color.gray4;
                                dock.style.borderRightColor = Color.gray4;

                                if (propertiesField != null)
                                {
                                    var propertiesFoldout = new Foldout()
                                    {
                                        text = "Properties",
                                        value = false,
                                        style =
                                        {
                                            width = Length.Percent(100),
                                            flexGrow = 0,
                                            flexShrink = 0
                                        }
                                    };

                                    var propertiesObject = (ScriptableObject)propertiesField.GetValue(component);

                                    if (propertiesObject == null && propertiesField != null)
                                    {
                                        propertiesObject = SchematicAssetManager.Create(component, "", "", "Properties", propertiesField.FieldType);

                                        var dirPath = Path.GetDirectoryName(_assetPath);

                                        var tfPath = prefab.transform.GetRelativePath(tf);
                                        var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                                        var tfCopy = copy.transform.Find(tfPath);
                                        if (tfCopy != null)
                                        {
                                            var componentCopy = tfCopy.gameObject.GetComponent(type);
                                            propertiesField.SetValue(componentCopy, propertiesObject);

                                            ApplyChangesToPrefabAsset(copy, goFoldout, false);
                                        }
                                    }

                                    var container = new IMGUIContainer(() =>
                                    {
                                        var soEditor = Editor.CreateEditor(propertiesObject);
                                        soEditor.OnInspectorGUI();
                                    });

                                    propertiesFoldout.Add(container);
                                    dock.Add(propertiesFoldout);
                                }
                            }
                            else
                            {
                                dock.style.display = DisplayStyle.None;
                                continue;
                            }


                            //content.Add(objectField);
                            content.Add(dock);
                        }


                        tabContentContainer.Add(content);
                        tabContentMap[typeName] = content;

                        // === Component Tab ===
                        var tabButton = new Button() { text = "", tooltip = typeName };

                        tabButton.clickable = new Clickable(() => 
                        { 
                            // Hide others
                            foreach (var tab in tabContentMap) tab.Value.style.display = DisplayStyle.None; 
                            // Show selected
                            content.style.display = DisplayStyle.Flex; selectedTab = typeName;

                            foreach (var child in tabBar.Children()) 
                            { 
                                child.style.flexDirection = FlexDirection.Row; 
                                child.style.alignContent = Align.Center; 
                                child.style.alignItems = Align.Center; 
                                child.style.backgroundColor = Color.gray3; 
                                child.style.borderBottomWidth = 0;
                                child.style.borderTopWidth = 0;
                                child.style.borderLeftWidth = 1; 
                                child.style.borderRightWidth = 1; 
                                child.style.borderTopLeftRadius = 0; 
                                child.style.borderTopRightRadius = 0; 
                                child.style.borderLeftColor = Color.black;
                                child.style.borderRightColor = Color.black;
                            }
                            content.style.flexDirection = FlexDirection.Column;

                            tabButton.style.backgroundColor = Color.gray1; 
                            tabButton.style.borderBottomWidth = 1; 
                            tabButton.style.borderTopWidth = 2; 
                            tabButton.style.borderLeftWidth = 2; 
                            tabButton.style.borderRightWidth = 2;
                            tabButton.style.borderBottomColor = Color.black;
                            tabButton.style.borderTopColor = Color.gray4;
                            tabButton.style.borderLeftColor = Color.gray4;
                            tabButton.style.borderRightColor = Color.gray4;
                            tabButton.style.borderTopLeftRadius = 4;
                            tabButton.style.borderTopRightRadius = 4; 
                        });

                        if (compCount == 0) 
                        {
                            tabButton.style.backgroundColor = Color.gray1;
                            tabButton.style.borderBottomWidth = 1;
                            tabButton.style.borderTopWidth = 2;
                            tabButton.style.borderLeftWidth = 2;
                            tabButton.style.borderRightWidth = 2;
                            tabButton.style.borderBottomColor = Color.black;
                            tabButton.style.borderTopColor = Color.gray4;
                            tabButton.style.borderLeftColor = Color.gray4; 
                            tabButton.style.borderRightColor = Color.gray4; 
                            tabButton.style.borderTopLeftRadius = 4;
                            tabButton.style.borderTopRightRadius = 4; 
                        };

                        tabButton.style.flexDirection = FlexDirection.Row;
                        tabButton.style.alignItems = Align.Center; 
                        tabButton.style.paddingTop = 0;
                        tabButton.style.paddingBottom = 0; 
                        tabButton.style.paddingLeft = 0;
                        tabButton.style.paddingRight = 0; 
                        tabButton.style.marginRight = 0;
                        tabButton.style.marginLeft = 0;
                        tabButton.style.marginTop = 0;
                        tabButton.style.marginBottom = 0;

                        var img = new Image { image = icon };
                        img.style.width = 16;
                        img.style.height = 16;

                        if (!schematicWindow.ShowComponentNames) 
                        {
                            img.style.marginBottom = 4; 
                            img.style.marginLeft = 4; 
                            img.style.marginTop = 4;
                            img.style.marginRight = 4;
                        }
                        var label = new Label(typeName);
                        label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        tabButton.Add(img);

                        if (schematicWindow.ShowComponentNames)
                            tabButton.Add(label);

                        var removeComponentButton = new Button(() =>
                        {
                            var tfPath = prefab.transform.GetRelativePath(tf);
                            var copy = PrefabUtility.LoadPrefabContents(_assetPath);
                            var tfCopy = copy.transform.Find(tfPath);
                            if (tfCopy != null)
                            {
                                var component = tfCopy.gameObject.GetComponent(type);

                                SchematicEditorData.DeleteComponentData(copy, tfPath, type);

                                Component.DestroyImmediate(component);

                                ApplyChangesToPrefabAsset(copy, goFoldout);

                            }
                        })
                        {
                            text = "-"
                        };

                        removeComponentButton.style.minWidth = 10;
                        removeComponentButton.style.minHeight = 10;
                        removeComponentButton.style.maxWidth = 10;
                        removeComponentButton.style.maxHeight = 10;

                        removeComponentButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"];
                        removeComponentButton.RegisterCallback<MouseEnterEvent>(evt => removeComponentButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["RemoveHighlight"]);
                        removeComponentButton.RegisterCallback<MouseLeaveEvent>(evt => removeComponentButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"]);

                        tabButton.Add(removeComponentButton);

                        tabBar.Add(tabButton); compCount++; 
                    }

                    // === Add Component "+" Tab ===
                    var addTabButton = new Button(() =>
                    {
                        var menu = new GenericMenu();
                        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t =>
                                typeof(MonoBehaviour).IsAssignableFrom(t) &&
                                (
                                    (typeof(DefaultIODockRenderer) != IODockRegistry.GetComponentRenderer(t).GetType()) ||
                                    (t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Any(f => typeof(ScriptableEventBase.IOBase).IsAssignableFrom(f.FieldType))) ||
                                    (t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Any(f => typeof(ScriptableEventBase.IOBase).IsAssignableFrom(f.PropertyType)))
                                )
                            );

                        foreach (var type in allTypes)
                        {
                            var attr = type.GetCustomAttribute<SchematicComponentAttribute>();

                            // If it has an attribute, use that path
                            string menuPath = attr != null ? $"{attr.Path}" : "Unsorted/" + type.FullName;

                            menu.AddItem(new GUIContent(menuPath), false, () =>
                            {
                                var tfPath = prefab.transform.GetRelativePath(tf);
                                var copy = PrefabUtility.LoadPrefabContents(_assetPath);
                                var tfCopy = copy.transform.Find(tfPath);

                                if (tfCopy != null)
                                {
                                    var component = tfCopy.gameObject.AddComponent(type);

                                    var ogComponent = tfCopy.gameObject.GetComponent(type);

                                    ApplyChangesToPrefabAsset(copy, goFoldout);
                                }
                            });
                        }


                        menu.ShowAsContext();

                    })
                    {
                        text = "+"
                    };

                    addTabButton.style.borderBottomLeftRadius = 0;
                    addTabButton.style.borderBottomRightRadius = 0;
                    addTabButton.style.borderTopLeftRadius = 0;
                    addTabButton.style.borderTopRightRadius = 0;
                    addTabButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["CreateHighlight"];

                    tabBar.Add(addTabButton);
                }

                // Auto-select first tab
                if (tabContentMap.Count > 0)
                {
                    var first = tabContentMap.Keys.First();
                    tabContentMap[first].style.display = DisplayStyle.Flex;
                    selectedTab = first;
                    tabBar.ElementAt(0).AddToClassList("selected-tab");
                }

                foldout.Add(tabScrollBar);
                foldout.Add(tabContentContainer);
            }

            PrefabRefreshUtility.ReimportAndResetPrefab(prefab);
        }


        Add(container);

        async void ApplyChangesToPrefabAsset(GameObject copy, GameObjectFoldout goFoldout, bool redraw = true)
        {
            PrefabUtility.SaveAsPrefabAsset(copy, _assetPath);
            PrefabUtility.UnloadPrefabContents(copy);

            AssetDatabase.Refresh();

            await EditorUtilities.NextEditorFrame();

            if(redraw)
            {
                if (schematicWindow == null)
                    rebuildHierarchy();
                else
                    schematicWindow.RedrawIODock();
            }

            if(goFoldout != null)
                goFoldout.Foldout.value = true;
        }

    }
}