using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.SceneManagement;

using System.IO;
using Remedy.Common;

#pragma warning disable

public static class SceneGenerator
{
    static GameObject activeEditedClone;
    static GameObject originalPrefabAsset;
    static string activeTempScenePath;

    [MenuItem("GameObject/Convert To Scene Asset", false, 10)]
    static void ConvertToSceneAsset(MenuCommand command)
    {
        GameObject go = command.context as GameObject;
        if (go == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        string folder = "Assets/GeneratedScenes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "GeneratedScenes");

        string path = Path.Combine(folder, go.name + ".unity");

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        GameObject clone = Object.Instantiate(go);
        clone.name = go.name;
        EditorSceneManager.MoveGameObjectToScene(clone, newScene);

        EditorSceneManager.SaveScene(newScene, path);
        AssetDatabase.Refresh();

        // Replace original object with SceneReference component
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        var reference = go.AddComponent<SceneReference>();
        reference.sceneAsset = sceneAsset;
        reference.loadOnStart = false;

        Debug.Log($"Scene asset created at {path} and linked to original GameObject.");
    }

    [MenuItem("GameObject/Edit Prefab In Place", false, 11)]
    static void EditPrefabInPlace(MenuCommand command)
    {
        GameObject prefabInstance = command.context as GameObject;
        if (prefabInstance == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstance);
        if (prefabAsset == null)
        {
            Debug.LogWarning("Selected object is not part of a prefab instance.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(prefabAsset);
        string tempScenePath = "Assets/Temp/" + prefabAsset.name + "_EditScene.unity";

        if (!AssetDatabase.IsValidFolder("Assets/Temp"))
            AssetDatabase.CreateFolder("Assets", "Temp");

        Scene tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        EditorSceneManager.MoveGameObjectToScene(clone, tempScene);
        EditorSceneManager.SaveScene(tempScene, tempScenePath);
        EditorSceneManager.OpenScene(tempScenePath, OpenSceneMode.Additive);

        Selection.activeObject = clone;

        activeEditedClone = clone;
        originalPrefabAsset = prefabAsset;
        activeTempScenePath = tempScenePath;

        SceneView.duringSceneGui += DrawOverlayGUI;
    }

    static void DrawOverlayGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 250, 100), "Editing Prefab", GUI.skin.window);
        GUILayout.Label(originalPrefabAsset.name);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Changes"))
        {
            ApplyChanges();
        }
        if (GUILayout.Button("Revert Changes"))
        {
            RevertChanges();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
        Handles.EndGUI();
    }

    static void ApplyChanges()
    {
        string originalPath = AssetDatabase.GetAssetPath(originalPrefabAsset);
        PrefabUtility.SaveAsPrefabAsset(activeEditedClone, originalPath);
        CleanupEditingSession();
        Debug.Log("Changes applied to prefab.");
    }

    static void RevertChanges()
    {
        CleanupEditingSession();
        Debug.Log("Changes reverted.");
    }

    static void CleanupEditingSession()
    {
        if (!string.IsNullOrEmpty(activeTempScenePath))
        {
            var scene = SceneManager.GetSceneByPath(activeTempScenePath);
            if (scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
                AssetDatabase.DeleteAsset(activeTempScenePath);
            }
        }

        activeEditedClone = null;
        originalPrefabAsset = null;
        activeTempScenePath = null;

        SceneView.duringSceneGui -= DrawOverlayGUI;
    }
}
