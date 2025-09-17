using Remedy.Framework;
//using SaintsField;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField]
        private Camera _camera;
        public static Camera Camera => Instance._camera;

        public List<UIComponent> Components = new();
        //[Layer]
        public int UILayer;
        public float BaseDepth = 50f;

        void OnEnable()
        {
            Components = new();
        }

        /// <summary>
        /// Adds a UIComponent instance to the list of managed Components
        /// </summary>
        /// <param name="component"></param>
        public static void AddUIComponent(UIComponent component)
        {
            component.gameObject.layer = Instance.UILayer;
            Vector3 position = component.transform.position;
            position.z = Instance.BaseDepth + component.Layer;
            component.transform.position = position;
            Instance.Components.Add(component);
        }

        /// <summary>
        /// Removes a UIComponent instance from the list of managed Components
        /// </summary>
        /// <param name="component"></param>
        public static void RemoveUIComponent(UIComponent component)
        {
            Instance.Components.Remove(component);

                component.gameObject.layer = Instance.UILayer;
                Vector3 position = component.transform.position;
                position.z = Instance.BaseDepth + component.Layer;
                component.transform.position = position;
        }
    }
}