using BlueGraph.Editor;
using Remedy.Schematics;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SchematicHierarchyDrawer
{
    static SchematicHierarchyDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        var controller = go.GetComponent<SchematicInstanceController>();
        if (controller == null/* || controller?.SchematicGraphs.Length > 0*/) return;

        foreach(var graph in controller.SchematicGraphs)
        {
            if (graph == null || controller == null || controller.gameObject == null) continue;
            graph.Prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
        }

        Rect buttonRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 18, selectionRect.height);
        if (GUI.Button(buttonRect, "§", EditorStyles.miniButton))
        {
            Selection.activeObject = controller.SchematicGraphs[0];
            EditorGUIUtility.PingObject(controller.SchematicGraphs[0]);
            GraphAssetHandler.OnOpenGraph(controller.SchematicGraphs[0]);
        }
    }
}
