using UnityEngine;
using UnityEngine.UIElements;

public interface IIODockRenderer
{
    public static Color DockColor => Color.gray1 + new Color(0.025f, 0.025f, 0.025f);
    public (bool, VisualElement) Render(SchematicGraphEditorWindow window, Object target, Transform prefabTransform, string specifiedPath = "");
}
