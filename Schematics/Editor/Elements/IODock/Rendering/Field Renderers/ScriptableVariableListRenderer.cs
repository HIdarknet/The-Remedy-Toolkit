using Remedy.Framework;
using Remedy.Schematics.Utils;
using SchematicAssets;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[FieldRendererTarget(typeof(ScriptableVariableListAttribute))]
public class ScriptableVariableListRenderer : FieldRenderer
{
    private List<ScriptableVariable> _variableList = new();

    public ScriptableVariableListRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, MemberWrapper fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModified) : base(window, obj, parent, target, fieldInfo, attr, path, onModified)
    {
        style.flexDirection = FlexDirection.Column;

        _variableList = (List<ScriptableVariable>)target;

        style.paddingBottom = 4;
        style.paddingLeft = 4;
        style.paddingTop = 4;
        style.paddingRight = 4;

        style.backgroundColor = Color.black;

        // List
        Add(CreateList(_variableList));
        Add(CreateNewVariableContainer());
    }

    private VisualElement CreateList(List<ScriptableVariable> variables)
    {
        var listContainer = new VisualElement()
        {
            style =
            {
                backgroundColor = Color.gray1,

                borderBottomWidth = 1,
                borderRightWidth = 1,
                borderLeftWidth = 1,
                borderTopWidth = 1,

                borderBottomRightRadius = 15,
                borderBottomLeftRadius = 15,
                borderTopRightRadius = 15,
                borderTopLeftRadius = 15,

                borderBottomColor = Color.white,
                borderRightColor = Color.white,
                borderLeftColor = Color.white,
                borderTopColor = Color.white
            }
        };

        foreach (var variable in variables)
        {
            listContainer.Add(CreateRow(variable));
        }

        return listContainer;
    }

    private VisualElement CreateRow(ScriptableVariable variable)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.marginBottom = 4;

        // Name field
        var nameField = new TextField { value = variable.name };
        nameField.style.flexGrow = 1;
        nameField.RegisterValueChangedCallback(evt =>
        {
            variable.name = evt.newValue;
            EditorUtility.SetDirty(variable);
        });
        row.Add(nameField);

        // Type dropdown
        var typeField = new EnumField(variable.Value.Type);
        typeField.style.width = 100;
        typeField.RegisterValueChangedCallback(evt =>
        {
            variable.Value.Type = (Union.ValueType)evt.newValue;
            EditorUtility.SetDirty(variable);
            RefreshValueField(row, variable);
        });
        row.Add(typeField);

        // Value field
        var valueField = CreateValueField(variable);
        valueField.style.flexGrow = 1;
        row.Add(valueField);

        // Drag Handle
        var dragLabel = new Label("☰") { style = { unityTextAlign = TextAnchor.MiddleCenter, alignSelf = Align.Center } };
        dragLabel.style.cursor = new StyleCursor(UnityDefaultCursor.DefaultCursor(UnityDefaultCursor.CursorType.Link));
        dragLabel.RegisterCallback<MouseDownEvent>(evt =>
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new UnityEngine.Object[] { variable };
            DragAndDrop.StartDrag("Drag ScriptableEvent");
            evt.StopPropagation();
        });

/*        dragLabel.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (SchematicGraphEditorWindow.ColorLookupValueType.ContainsKey(argType))
            {
                row.style.borderBottomColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                row.style.borderRightColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                row.style.borderTopColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
                row.style.borderLeftColor = SchematicGraphEditorWindow.ColorLookupValueType[argType];
            }
        });

        dragLabel.RegisterCallback<MouseLeaveEvent>(evt =>
        {

            row.style.borderBottomColor = new Color(0, 0, 0, 0);
            row.style.borderRightColor = new Color(0, 0, 0, 0);
            row.style.borderTopColor = new Color(0, 0, 0, 0);
            row.style.borderLeftColor = new Color(0, 0, 0, 0);
        });*/

        return row;
    }

    private void RefreshValueField(VisualElement row, ScriptableVariable variable)
    {
        // Remove old value field
        if (row.childCount > 2)
            row.RemoveAt(2);

        // Add updated one
        var newValueField = CreateValueField(variable);
        newValueField.style.flexGrow = 1;
        row.Add(newValueField);
    }

    private VisualElement CreateValueField(ScriptableVariable variable)
    {
        switch (variable.Value.Type)
        {
            case Union.ValueType.Float:
                var floatField = new FloatField { value = variable.Value };
                floatField.RegisterValueChangedCallback(evt =>
                {
                    variable.Value = evt.newValue;
                    EditorUtility.SetDirty(variable);
                });
                return floatField;

            case Union.ValueType.Int:
                var intField = new IntegerField { value = variable.Value };
                intField.RegisterValueChangedCallback(evt =>
                {
                    variable.Value = evt.newValue;
                    EditorUtility.SetDirty(variable);
                });
                return intField;

            case Union.ValueType.Bool:
                var boolField = new Toggle { value = variable.Value };
                boolField.RegisterValueChangedCallback(evt =>
                {
                    variable.Value = evt.newValue;
                    EditorUtility.SetDirty(variable);
                });
                return boolField;

            case Union.ValueType.String:
                var stringField = new TextField { value = variable.Value };
                stringField.RegisterValueChangedCallback(evt =>
                {
                    variable.Value = evt.newValue;
                    EditorUtility.SetDirty(variable);
                });
                return stringField;

            default:
                return new Label("Unsupported type");
        }
    }

    private VisualElement CreateNewVariableContainer()
    {
        var newVariableContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Row
            }
        };

        var newVariableName = new TextField()
        {
            style =
            {
                flexGrow = 1
            }
        };

        var newVariableButton = new Button(() =>
        {
            if(!SchematicAssetManager.AssetExists(_object, "Variables", "", newVariableName.value))
            {
                var createdAsset = SchematicAssetManager.Create<ScriptableVariable>(_object, "Variables", "", newVariableName.value);
                _variableList.Add(createdAsset);
                _field.SetValue(_parent, _variableList);
                AttemptPrefabRefresh();
            }
        })
        {
            text = "Add"
        };

        newVariableContainer.Add(newVariableName);
        newVariableContainer.Add(newVariableButton);

        return newVariableContainer;
    }
}
