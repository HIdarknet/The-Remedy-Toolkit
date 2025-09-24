using Remedy.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEngine.TextCore.Text;

[FieldRendererTarget(typeof(EventContainerRendererAttribute))]
public class EventContainerRenderer : FieldRenderer
{
    private static SerializableDictionary<(Type type, string fieldName), ScriptableEventReference> _cachedReferences = new();
    private static List<EventContainerRenderer> _eventLinkWaitPool = new(); // Events waiting for the rest of the Component to finish displaying before attempting to draw
    private EventContainerRendererAttribute _evAttr;
    private EventLinkAttribute _linkAttr;

    public EventContainerRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, MemberWrapper fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModified) : base(window, obj, parent, target, fieldInfo, attr, path, onModified)
    {
        _evAttr = (EventContainerRendererAttribute)attr;
        _linkAttr = fieldInfo?.GetCustomAttributes()?.OfType<EventLinkAttribute>().FirstOrDefault();

        if (_evAttr == null)
        {
            var defType = _field.MemberType.GetGenericArguments().FirstOrDefault();

            if (defType != null)
                _evAttr = new(ScriptableEventBase.GetEventTypeForArgumentType(defType), _field.Name, true, false);
            else
                _evAttr = new(typeof(ScriptableEvent), _field.Name, true, true);
        }

        if (_linkAttr != null)
            DrawEventContainer();
        else
            _eventLinkWaitPool.Add(this);
    }

    /// <summary>
    /// Draws the Events that were waiting for the rest of the Component to render before they themselves rendered.
    /// </summary>
    public static void DrawDelayedEvents()
    {
        foreach(var eventContainer in _eventLinkWaitPool)
        {
            eventContainer.DrawEventContainer();
        }
        _eventLinkWaitPool.Clear();
    }

    protected void DrawEventContainer()
    {
        var column = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                backgroundColor = new Color(0, 0, 0, 0.1f),

                borderBottomColor = Color.gray4,
                borderTopColor = Color.gray4,

                borderLeftWidth = 2,
                borderRightWidth = 2,
                borderTopWidth = 1,
                borderBottomWidth = 1,

                borderRightColor = Color.black,
                borderLeftColor = Color.black
            }
        };

        var row = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                marginTop = 2,
                marginBottom = 2,
                paddingBottom = 2,
                paddingTop = 2,
                borderBottomWidth = 0,
                paddingLeft = 0
            }
        };

        ScriptableEventReference ser = null;
        bool loadedFromOther = false;
        
        if(_linkAttr != null)
        {
            if(_cachedReferences.TryGetValue((_linkAttr.Type, _linkAttr.FieldName), out ser))
            {
                loadedFromOther = true;

                var parentLabel = new Label
                {
                    text = "> " + _linkAttr.Type  + _linkAttr.FieldName,
                    style =
                    {
                        fontSize = 8,
                        color = Color.gray8,

                        marginTop = 2,
                        marginLeft = 2
                    }
                };

                column.Add(parentLabel);
            }
        }

        if(!loadedFromOther)
        {
            ser = _window.EventManager.GetOrCreateEventReference(_object, _path, _field, _evAttr.DefaultType, _evAttr.DefaultName);

            _cachedReferences.Add((_object.GetType(), _field.Name), ser);
        }

        // Name field
        var nameField = new TextField { value = ser.EventName, style = { flexGrow = 1, paddingLeft = 0 } };
        string oldName = nameField.value;
        string newName = nameField.value;

        nameField.RegisterValueChangedCallback(evt =>
        {
            newName = evt.newValue;
        });

        nameField.RegisterCallback<FocusInEvent>(evt =>
        {
            Undo.RecordObject(_window, "Edit Event Name");
            oldName = nameField.value;
            _window.Canvas.Reload();
        });

        nameField.RegisterCallback<FocusOutEvent>(evt =>
        {
            Undo.RecordObject(_window, "Edit Event Name");
            ser.ChangeName(oldName, newName);
            _window.Canvas.Reload();
        });

        var availableTypes = ScriptableEventBase.ScriptableEventTypes;
        Type selectedType = typeof(ScriptableEventBase);

        Type argType = null;
        var genArgs = ser.EventType.BaseType.GetGenericArguments();
        if (genArgs.Length > 0)
        {
            selectedType = ScriptableEventBase.GetEventTypeForArgumentType(genArgs[0]);
            argType = genArgs[0];
        }

        if (selectedType == typeof(ScriptableEventBase))
            selectedType = typeof(ScriptableEvent);

        // Type Dropdown
        var typePopup = new PopupField<Type>(
            ScriptableEventBase.ScriptableEventTypes.Where(t => t != typeof(ScriptableEventBase)).ToList(),
            selectedType,
            t => t.Name,
            t => t.Name
        );

        if (!_evAttr.CanChangeType)
        {
            typePopup.SetEnabled(false);
        }

        // Global toggle
        var globalToggle = new Toggle { value = ser.EventAsset.Global };

        globalToggle.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(_window, "Toggle Global");
            ser.EventAsset.Global = evt.newValue;
            _window.Canvas.Reload();
        });

        // Drag Handle
        var dragLabel = new Label("☰") { style = { unityTextAlign = TextAnchor.MiddleCenter, alignSelf = Align.Center } };
        dragLabel.style.cursor = new StyleCursor(UnityDefaultCursor.DefaultCursor(UnityDefaultCursor.CursorType.Link));
        dragLabel.RegisterCallback<MouseDownEvent>(evt =>
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new UnityEngine.Object[] { ser.EventAsset };
            DragAndDrop.StartDrag("Drag ScriptableEvent");
            evt.StopPropagation();
        });

        dragLabel.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (SchematicGraphEditorWindow.ColorLookupValueType.ContainsKey(argType))
            {
                column.style.borderBottomColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                column.style.borderRightColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                column.style.borderTopColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                column.style.borderLeftColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
            }
        });

        dragLabel.RegisterCallback<MouseLeaveEvent>(evt =>
        {

            column.style.borderBottomColor = Color.gray4;
            column.style.borderRightColor = new Color(0, 0, 0, 0);
            column.style.borderTopColor = Color.gray4;
            column.style.borderLeftColor = new Color(0, 0, 0, 0);
        });

        typePopup.RegisterValueChangedCallback(evt =>
        {
            //Debug.Log("Type changed to " + evt.newValue);
            ser.ChangeType(ScriptableEventBase.ScriptableEventTypes.IndexOf(evt.newValue));
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            _window.Canvas.Reload();

            Type argType = null;
            var genArgs = evt.newValue.BaseType.GetGenericArguments();
            if (genArgs.Length > 0)
            {
                selectedType = ScriptableEventBase.GetEventTypeForArgumentType(genArgs[0]);
                argType = genArgs[0];
            }

            dragLabel.RegisterCallback<MouseEnterEvent>(evt =>
            {
                column.style.borderBottomColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                column.style.borderRightColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                column.style.borderTopColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                column.style.borderLeftColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
            });

            dragLabel.RegisterCallback<MouseLeaveEvent>(evt =>
            {

                column.style.borderBottomColor = Color.gray4;
                column.style.borderRightColor = new Color(0, 0, 0, 0);
                column.style.borderTopColor = Color.gray4;
                column.style.borderLeftColor = new Color(0, 0, 0, 0);
            });
        });
        Texture2D myTexture;
        if (_field.MemberType.Name.Contains("Output"))
            myTexture = Resources.Load<Texture2D>("Icons/Output");
        else
            myTexture = Resources.Load<Texture2D>("Icons/Input");
        Image imageElement = new Image();
        imageElement.image = myTexture; // Use texture for UI Toolkit
        imageElement.style.width = 32;
        imageElement.style.height = 24;
        imageElement.style.opacity = 0.5f;

        row.Add(imageElement);
        row.Add(nameField);
        row.Add(typePopup);
        row.Add(globalToggle);

        //Remove button
        row.Add(dragLabel);

        if (argType != null && SchematicGraphEditorWindow.ColorLookupValueType.ContainsKey(argType))
        {
            column.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupValueType[argType] * new Color(0.8f, 0.8f, 0.8f, 0.1f);
        }

        column.Add(row);

        Add(column);
    }


    protected IMGUIContainer DrawGUIContainer()
    {
        SerializedObject seriializedObject = new SerializedObject(_object);

        var valPath = _path + "." + _field.Name + "." + nameof(ScriptableEvent.Input.OriginalValue);
        var property = seriializedObject.FindProperty(valPath);

        var imguiContainerContainer = new VisualElement();

        var imguiContainer = new IMGUIContainer()
        {
            style =
            {
                flexGrow = 0,
                flexShrink = 0
            }
        };

        imguiContainer.onGUIHandler = () =>
        {
            seriializedObject.Update();
            EditorGUILayout.PropertyField(property);
            seriializedObject.ApplyModifiedProperties();

            EditorApplication.QueuePlayerLoopUpdate();
            imguiContainer.MarkDirtyRepaint();
        };

        EditorApplication.update += () =>
        {
            if (imguiContainer == null) return;
            imguiContainer.MarkDirtyRepaint();
        };

        imguiContainer.focusable = true;
        imguiContainer.tabIndex = 0;

        return imguiContainer;
    }
}