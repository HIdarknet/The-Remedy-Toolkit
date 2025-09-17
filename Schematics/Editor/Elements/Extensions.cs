using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{ 
    /// <summary>
    /// Registers a color change when the user hovers over an element for visual feedback of cursor placement
    /// </summary>
    /// <param name="element"></param>
    /// <param name="original"></param>
    /// <param name="highlight"></param>
    public static void EnableInteractionHoverHighlight(this VisualElement element, Color original, Color highlight)
    {
        element.RegisterCallback<MouseEnterEvent>(evt => element.style.backgroundColor = highlight);
        element.RegisterCallback<MouseLeaveEvent>(evt => element.style.backgroundColor = original);
    }
}

