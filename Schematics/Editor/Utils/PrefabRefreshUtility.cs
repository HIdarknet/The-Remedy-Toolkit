using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PrefabRefreshUtility
{
    /// <summary>
    /// Reimports the prefab asset and resets the Prefab editor stage if it's open, preserving selection.
    /// </summary>
    public static void ReimportAndResetPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("No prefab provided to reimport/reset.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(prefab);

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"Provided object '{prefab.name}' is not a prefab asset.");
            return;
        }

        // Remember the current selection so we can restore it after reload
        Object previouslySelected = Selection.activeObject;

        // Reimport the prefab asset
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var currentStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (currentStage != null && currentStage.assetPath == path)
        {
            // Store path so we can reopen
            string assetPath = currentStage.assetPath;

            // Close the stage
            EditorSceneManager.CloseScene(currentStage.scene, true);

            // Reopen in Prefab Mode
            var reopenedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            AssetDatabase.OpenAsset(reopenedPrefab);

            // Restore selection
            if (previouslySelected != null)
                Selection.activeObject = previouslySelected;
        }
        else
        {
            // Restore selection
            if (previouslySelected != null)
                Selection.activeObject = previouslySelected;
        }
    }
}
