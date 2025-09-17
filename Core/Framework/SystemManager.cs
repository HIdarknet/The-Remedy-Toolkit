using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Remedy.Framework
{
    public static class SystemManager
    { 
        public static bool IsMultiplayerGame = false;

        /// <summary>
        /// A cache of components attached to game objects, indexed by type.
        /// </summary>
        private static Dictionary<GameObject, Dictionary<Type, Component>> _componentCache = new();

        private static Dictionary<Type, SingletonBase> _singletons = new();
        public static Dictionary<Type, SingletonBase> Singletons => new(_singletons);

        public static void AddSingleton(object singleton)
        {
            if (!_singletons.ContainsKey(singleton.GetType()))
                _singletons.Add(singleton.GetType(), (SingletonBase)singleton);
        }

        /// <summary>
        /// Attempts to get a Component from the Cache, or gets it from the object and adds it to the cache for later.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetCachedComponent<T>(GameObject obj) where T : Component
        {
            if (!_componentCache.TryGetValue(obj, out var componentDictionary))
            {
                componentDictionary = new Dictionary<Type, Component>();
                _componentCache[obj] = componentDictionary;
            }

            if (componentDictionary.TryGetValue(typeof(T), out var cachedComponent))
            {
                return (T)cachedComponent;
            }

            var component = obj.GetComponent<T>();
            componentDictionary[typeof(T)] = component;
            return component;
        }
    }
}

public static class ExtendedResources
{
    public static T[] LoadAllByComponent<T>(string path) where T : Component
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>(path);
        return gameObjects.SelectMany(go => go.GetComponents<T>()).ToArray();
    }
}
