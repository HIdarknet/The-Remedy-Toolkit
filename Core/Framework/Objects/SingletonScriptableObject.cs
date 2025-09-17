using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Remedy.Framework
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T cachedAsset;

        public static T Asset
        {
            get
            {
                if (cachedAsset == null)
                {
                    cachedAsset = Resources.Load<T>(typeof(T).Name);

                    if (cachedAsset == null)
                    {
                        cachedAsset = CreateInstance<T>();

#if UNITY_EDITOR
                        string resourcesPath = "Assets/Resources";
                        if (!Directory.Exists(resourcesPath))
                        {
                            Directory.CreateDirectory(resourcesPath);
                        }

                        string assetPath = $"{resourcesPath}/{typeof(T).Name}.asset";
                        AssetDatabase.CreateAsset(cachedAsset, assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log($"Created new instance of {typeof(T).Name} in Resources folder.");
#else
                    Debug.Log($"Created new instance of {typeof(T).Name} as it was not found in Resources.");
#endif
                    }
                    else
                    {
                        Debug.Log($"Loaded {typeof(T).Name} from Resources.");
                    }
                }

                return cachedAsset;
            }
        }
    }
}
