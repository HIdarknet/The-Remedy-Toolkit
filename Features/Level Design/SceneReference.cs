#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[ExecuteAlways]
public class SceneReference : MonoBehaviour
{
    public SceneAsset sceneAsset;
    public bool loadOnStart = true;

    [HideInInspector]
    public string scenePath;

    private Scene loadedScene;

    void Awake()
    {
        if (Application.isPlaying && loadOnStart)
            LoadScene();
#if UNITY_EDITOR
        else if (!Application.isPlaying && loadOnStart)
            EditorApplication.delayCall += () => LoadSceneInEditor();
#endif
    }

    public void LoadScene()
    {
        if (sceneAsset == null) return;

        scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        if (string.IsNullOrEmpty(scenePath)) return;

        if (!SceneManager.GetSceneByPath(scenePath).isLoaded)
        {
            SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive).completed += (op) =>
            {
                loadedScene = SceneManager.GetSceneByPath(scenePath);
            };
        }
    }

#if UNITY_EDITOR
    private void LoadSceneInEditor()
    {
        if (sceneAsset == null) return;

        scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        if (string.IsNullOrEmpty(scenePath)) return;

        if (!EditorSceneManager.GetSceneByPath(scenePath).isLoaded)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
    }

    void OnDisable()
    {
        if (!Application.isPlaying && sceneAsset != null && !string.IsNullOrEmpty(scenePath))
        {
            var scene = EditorSceneManager.GetSceneByPath(scenePath);
            if (scene.IsValid() && scene.isDirty)
                EditorSceneManager.SaveScene(scene);
        }
    }
#endif
}