using UnityEditor;
//using SaintsField.Editor;
//using SaintsField;
using UnityEngine;

public class ScriptableObjectEditorWindow : EditorWindow
{
    //[Expandable]
    public ScriptableObject Data;

    public static ScriptableObjectEditorWindow OpenWindow(ScriptableObject data)
    {
        ScriptableObjectEditorWindow window = GetWindow<ScriptableObjectEditorWindow>(false, data.name);
        window.Show();
        window.Data = data;
        return window;
    }
}
