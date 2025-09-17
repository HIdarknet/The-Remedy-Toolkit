using UnityEngine;
using UnityEditor;

namespace Remedy.Framework
{
    public class SingletonDataBase : ScriptableObject
    { }

    /// <summary>
    /// A simple Singleton MonoBehaviour which can be accessed statically via the Instance property.
    /// </summary>
    /// <typeparam name="TSingletonData">This Singleton Class (for accessing the Singleton statically).</typeparam>
    public class SingletonData<TSingletonData> : SingletonDataBase where TSingletonData : SingletonData<TSingletonData>
    {
        private static TSingletonData _instance;

        /// <summary>
        /// A static property that returns the singleton instance.
        /// If the instance does not exist, it will try to get it from the game object or add it as a component.
        /// </summary>
        public static TSingletonData Instance
        {
            get
            {
                // Check if the instance is already assigned
                if (_instance == null)
                {
                    // Try to find it in Resources
                    TSingletonData[] foundInstances = Resources.LoadAll<TSingletonData>("");

                    // If instances are found, use the first one
                    if (foundInstances.Length > 0)
                    {
                        _instance = foundInstances[0];
                    }
                    else
                    {
                        // If not found in Resources, create a new one if in Editor
#if UNITY_EDITOR
                        _instance = ScriptableObject.CreateInstance<TSingletonData>();

                        if (!AssetDatabase.IsValidFolder("Assets/Content"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Content");
                        }
                        if (!AssetDatabase.IsValidFolder("Assets/Content/Resources"))
                        {
                            AssetDatabase.CreateFolder("Assets/Content", "Resources");
                        }
                        if (!AssetDatabase.IsValidFolder("Assets/Content/Resources/Data"))
                        {
                            AssetDatabase.CreateFolder("Assets/Content/Resources", "Data");
                        }

                        // Save the new asset in the Resources/Data folder
                        string assetPath = "Assets/Content/Resources/Data/" + typeof(TSingletonData).Name + ".asset";
                        AssetDatabase.CreateAsset(_instance, assetPath);
                        AssetDatabase.SaveAssets();
#endif
                    }
                }
                return _instance;
            }
        }
    }
}
