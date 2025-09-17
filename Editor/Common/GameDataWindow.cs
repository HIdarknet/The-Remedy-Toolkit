using UnityEditor;
using UnityEngine;
//using SaintsField.Editor;
//using SaintsField;
//using SaintsField.ComponentHeader;
//using SaintsField.Playa;
using Remedy.Common;

public class GameDataWindow : EditorWindow
{
    //[Button(label: "🔃 Reload Assets")]
    /// <summary>
    /// Searches through the Assets folder for all Components to display in Game Data.
    /// </summary>
    public void ReloadAssets()
    {
        GameData.ReloadAssets();
    }

    //[Expandable]
    public GameData GameData;

    [MenuItem("Tools/Game Data")]
    public static void OpenWindow()
    {
        EditorWindow window = GetWindow<GameDataWindow>(false, "Game Data");
        window.Show();
    }/*
    public override void OnEditorEnable()
    {
        GameData = GameData.Instance;
        ReloadAssets();
    }*/

    // life-circle: OnUpdate function
    /*    public override void OnEditorUpdate()
        {
            myProgress = (myProgress + 1f) % 100;
        }
    */
    // Other life-circle
    /*    public override void OnEditorEnable()
        {
            Debug.Log("Enable");
        }

        public override void OnEditorDisable()
        {
            Debug.Log("Disable");
        }

        public override void OnEditorDestroy()
        {
            Debug.Log("Destroy");
        }*/
}
