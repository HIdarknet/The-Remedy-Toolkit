using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SchematicGenerationPrefabPostProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // --- CLEANUP PASS ---
        bool modified = false;

        if (modified)
        {
            EditorUtility.SetDirty(SchematicEditorData.Instance);
            AssetDatabase.SaveAssets();
        }

        // --- PREFAB PROCESSING ---
        foreach (string assetPath in importedAssets)
        {
            if (!assetPath.EndsWith(".prefab")) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Debug.Log(assetPath);
            if (prefab == null) continue;

            var created = SchematicGenerator.CreateSchematicForPrefab(prefab);

            if (created)
                SchematicEditorData.AddSchematicData(prefab);
        }

        SchematicEditorData.DeleteSchematicPrefabs(deletedAssets);
    }
}
