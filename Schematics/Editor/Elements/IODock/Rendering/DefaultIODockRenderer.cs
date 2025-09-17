using UnityEngine;
using UnityEngine.UIElements;

public class DefaultIODockRenderer : IIODockRenderer
{
    public virtual (bool, VisualElement) Render(SchematicGraphEditorWindow window, UnityEngine.Object target, Transform prefab, string specifiedPath = "")
    {
        var componentScrollView = new VisualElement
        {
            name = "IO Dock Scrollview",
            style =
            {
                flexDirection = FlexDirection.Column
            }
        };
        componentScrollView.contentContainer.style.flexDirection = FlexDirection.Column;


        (bool draw, VisualElement customFieldContainer) = IODockRegistry.RenderMultipleFields(window, target, target, displayObjectName: true); 
        
        if (customFieldContainer != null)
        {
            componentScrollView.Add(customFieldContainer);
/*            customFieldContainer.parent.style.flexDirection = FlexDirection.Column;
            customFieldContainer.style.flexDirection = FlexDirection.Column;*/
        }

        componentScrollView.style.marginTop = 0;
        componentScrollView.style.marginBottom = 0;

        return (draw, componentScrollView);
    }
}
