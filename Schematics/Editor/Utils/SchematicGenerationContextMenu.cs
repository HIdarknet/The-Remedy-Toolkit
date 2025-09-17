using SchematicAssets;
using UnityEditor;
using UnityEngine;

public static class PrefabContextMenu
{
    [MenuItem("Assets/Transform Into Schematic", true)]
    private static bool ValidateTransformIntoSchematic()
    {
        GameObject prefab = Selection.activeObject as GameObject;
        if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            return false;
        return SchematicAssetManager.ObjectFolderExists(prefab);
    }

    [MenuItem("Assets/Transform Into Schematic")]
    private static void TransformIntoSchematic()
    {
        GameObject prefab = Selection.activeObject as GameObject;
        SchematicGenerator.CreateSchematicForPrefab(prefab);
    }

    [MenuItem("Assets/Open Schematic", true)]
    private static bool ValidateOpenSchematic()
    {
        GameObject prefab = Selection.activeObject as GameObject;
        if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            return false;

        return SchematicAssetManager.ObjectFolderExists(prefab);
    }

    [MenuItem("Assets/Open Schematic")]
    private static void OpenSchematic()
    {
        GameObject prefab = Selection.activeObject as GameObject;
        AssetDatabase.OpenAsset(prefab);

        SchematicGenerator.OpenSchematicForPrefab(prefab);
    }
}
