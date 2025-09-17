using Remedy.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class VariableContainer : VisualElement
{
/*
    /// <summary>
    /// Draws a UI Container that contains ScriptableVariableReferences
    /// </summary>
    /// <param name="modifiable">If the Variables can be Modified by the user (false if they were received through reflection)</param>
    /// <returns></returns>
    private VisualElement DrawVariableContainer(SchematicScope.ScriptableVariableReference[] variables, bool modifiable = true)
    {
        var variablesContainer = new VisualElement();

        var varHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center, alignItems = Align.Center } };
        varHeader.Add(new Label("Variables") { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });

        if (modifiable)
        {
            var addVarButton = new Button(() =>
            {
                Undo.RecordObject(Schematic, "Add Variable Reference");
                var list = Schematic.Variables.ToList();
                list.Add(new SchematicScope.ScriptableVariableReference());
                Schematic.Variables = list.ToArray();
                EditorUtility.SetDirty(Schematic);
                RedrawIODock();
            })
            { text = "+" };

            varHeader.Add(addVarButton);
        }

        variablesContainer.Add(varHeader);

        for (int i = 0; i < variables.Length; i++)
        {
            var row = DrawVariableRow(variables[i], modifiable);
            variablesContainer.Add(row);
        }

        return variablesContainer;
    }

    /// <summary>
    /// Creates the UI Element for the ScriptableVariable Reference
    /// </summary>
    /// <param name="v"></param>    
    /// <param name="modifiable">Whether the Variable can be Modified by the user (those recieved through reflection should not be modifiable)</param>
    /// <returns>The created VisualElement</returns>
    private VisualElement DrawVariableRow(SchematicScope.ScriptableVariableReference v, bool modifiable = true)
    {
        var row = new VisualElement
        {
            style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 2,
                    marginBottom = 2,
                    paddingBottom = 2,
                    paddingTop = 2,
                    backgroundColor = new Color(0, 0, 0, 0.1f),
                    borderBottomColor = new Color(0, 0, 0, 0.3f),
                    borderBottomWidth = 0
                }
        };

        var nameField = new TextField { value = v.Name, style = { flexGrow = 1 } };
        nameField.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(Schematic, "Edit Variable Name");
            v.Name = evt.newValue;
            EditorUtility.SetDirty(Schematic);
        });

        var selectedType = v.Type;

        var typePopup = new PopupField<Type>(
            ScriptableVariable.ScriptableVariableTypes,
            selectedType,
            t => t.Name,
            t => t.Name
        );

        typePopup.RegisterValueChangedCallback(evt =>
        {
            v.Type = ScriptableVariable.ScriptableVariableTypes.IndexOf(evt.newValue);
            EditorUtility.SetDirty(Schematic);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        });

        var globalToggle = new Toggle { value = v.Global };
        globalToggle.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(Schematic, "Toggle Variable Global");
            v.Global = evt.newValue;
            EditorUtility.SetDirty(Schematic);
        });

        var removeBtn = new Button(() =>
        {
            Undo.RecordObject(Schematic, "Remove Variable Reference");
            var list = Schematic.Variables.ToList();
            list.Remove(v);
            Schematic.Variables = list.ToArray();
            EditorUtility.SetDirty(Schematic);
            RedrawIODock();
        })
        { text = "-" };

        var dragLabel = new Label("☰") { style = { unityTextAlign = TextAnchor.MiddleCenter, alignSelf = Align.Center } };

        dragLabel.style.cursor = new StyleCursor(UnityDefaultCursor.DefaultCursor(UnityDefaultCursor.CursorType.Link));

        dragLabel.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button != 0) return;
            if (v.Variable == null) return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new UnityEngine.Object[] { v.Variable };
            DragAndDrop.StartDrag("Drag ScriptableVariable");
            evt.StopPropagation();
        });

        row.Add(nameField);
        row.Add(typePopup);
        row.Add(globalToggle);
        row.Add(removeBtn);
        row.Add(dragLabel);

        return row;
    }*/
}