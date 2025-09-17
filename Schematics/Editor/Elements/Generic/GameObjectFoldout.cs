using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class GameObjectFoldout : VisualElement
{
    public PersistentFoldout Foldout { get; private set; }
    public event Action<string> OnRenameRequested; // callback for renaming

    public GameObjectFoldout(GameObject gameObject, string GUID, string id, string labelText)
    {
        // Icon
        Texture2D icon = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;

        // Layout container
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.FlexStart;

        // Icon image
        var iconImage = new Image
        {
            image = icon,
            scaleMode = ScaleMode.ScaleToFit
        };
        iconImage.style.minWidth = 16;
        iconImage.style.minHeight = 16;
        iconImage.style.maxWidth = 16;
        iconImage.style.maxHeight = 16;

        // Foldout
        Foldout = new PersistentFoldout(GUID, id)
        {
            text = labelText,
            value = true,
            style =
            {
                flexDirection = FlexDirection.Column,
                flexGrow = 1,
                minHeight = 0,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        Foldout.style.marginTop = 0;

        var contentContainer = Foldout.Q<VisualElement>("unity-content");
        if (contentContainer != null)
        {
            contentContainer.style.marginLeft = 0;
            contentContainer.style.paddingLeft = 0;
        }

        // Add elements
        container.Add(iconImage);
        container.Add(Foldout);
        this.Add(container);

        // Enable inline renaming
        EnableInlineRename(Foldout);
    }

    public void Add(GameObjectFoldout goFoldout)
    {
        Foldout.Add(goFoldout);
    }

    private void EnableInlineRename(Foldout foldout)
    {
        var label = foldout.Q<Label>(); 

        if (label == null)
            return;

        label.RegisterCallback<MouseDownEvent>(evt =>
        {
            evt.StopImmediatePropagation();

            if (evt.clickCount == 2 && evt.button == 0) // Double left-click
            {
                BeginRename(label);
                evt.StopImmediatePropagation();
            }
        });
    }

    private void BeginRename(Label label)
    {
        string oldName = label.text;

        var textField = new TextField
        {
            value = oldName
        };
        textField.style.flexGrow = 1;
        textField.SelectAll();

        var parent = label.parent;
        int index = parent.IndexOf(label);
        parent.Insert(index, textField);
        label.RemoveFromHierarchy();

        // Confirm rename on enter or blur
        void EndRename()
        {
            string newName = textField.value.Trim();
            if (!string.IsNullOrEmpty(newName) && newName != oldName)
            {
                OnRenameRequested?.Invoke(newName); 
            }

            label.text = newName;
            parent.Insert(index, label);
            textField.RemoveFromHierarchy();
        }

        textField.RegisterCallback<FocusOutEvent>(_ => EndRename());
        textField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                EndRename();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                // Cancel
                parent.Insert(index, label);
                textField.RemoveFromHierarchy();
                evt.StopPropagation();
            }
        });

        textField.Focus();
    }
}
