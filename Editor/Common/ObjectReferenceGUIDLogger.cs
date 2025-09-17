using UnityEngine;
using UnityEditor;

public static class ObjectReferenceGUIDLogger
{
    [MenuItem("CONTEXT/Object/Debug/Log Reference GUID")]
    private static void LogReferenceGUID(MenuCommand command)
    {
        Object obj = command.context;
        if (obj == null)
        {
            Debug.LogWarning("No object reference found.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"Object '{obj.name}' is not an asset, so it has no GUID.");
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(path);
        Debug.Log($"Object: {obj.name}\nPath: {path}\nGUID: {guid}");
    }
}
