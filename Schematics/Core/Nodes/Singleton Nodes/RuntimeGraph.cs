// project armada

#pragma warning disable 0414

using Remedy.Framework;
using Remedy.Schematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using BlueGraph;

[CreateAssetMenu(fileName = "New Runtime Graph", menuName = "Runtime/Runtime Graph"), IncludeTags("Default", "Runtime")]
public class RuntimeGraph : SchematicGraph
{
    public override string Title
    {
        get { return "RUNTIME"; }
    }
    private static RuntimeGraph _instance;
    public static RuntimeGraph Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the RuntimeGraph asset from Resources
                var runtimeGraphs = Resources.LoadAll<RuntimeGraph>("");
                if (runtimeGraphs.Length == 0)
                    _instance = null;
                else
                    _instance = runtimeGraphs[0];
            }
            return _instance;
        }
    }

    // This method will be called when the game starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnRuntimeMethodLoad()
    {
        Instance?.TriggerEvent<OnAfterFirstSceneLoaded>();

        if(Instance != null)
        {
            // Update using the System Manager

            // TODO: Rewire to Singleton Events:
/*            SystemManager.Instance.OnUpdate += () => Instance.TriggerEvent<OnRuntimeUpdate>();
            SystemManager.Instance.OnLateUpdate += () => Instance.TriggerEvent<OnRuntimeLateUpdate>();
            SystemManager.Instance.OnFixedUpdate += () => Instance.TriggerEvent<OnRuntimeFixedUpdate>();*/

            // Scene functionality using the Scene Manager
            SceneManager.sceneLoaded  += (Scene scene, LoadSceneMode mode)  => Instance.TriggerEvent<OnSceneLoaded>(scene);
            SceneManager.sceneUnloaded += (Scene scene) => Instance.TriggerEvent<OnSceneUnloaded>(scene);
            SceneManager.activeSceneChanged += (Scene lastScene, Scene newScene) => Instance.TriggerEvent<OnActiveSceneChanged>(lastScene, newScene);

            // Level functionality using the Level Manager
/*            LevelEventHandler.OnLevelStart += (string levelName) => Instance.TriggerEvent<OnLevelStart>(levelName);
            LevelEventHandler.OnLevelExit += (string levelName) => Instance.TriggerEvent<OnLevelExit>(levelName);*/
        }
    }

    // This method will be called before the first scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        Instance?.TriggerEvent<OnBeforeFirstSceneLoaded>();
    }
}
