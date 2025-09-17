//using SaintsField;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.UI
{
    public class UIComponent : MonoBehaviour
    {
        [Tooltip("The UI Layer to render this Prefab's Canvas on.")]
        public int Layer;

        private void Start()
        {
            UIManager.AddUIComponent(this);
            InitiateAndSubscribeEvents();
        }

        private void OnDisable()
        {
            UIManager.RemoveUIComponent(this);
            UnSubscribeEvents();
        }

        protected virtual void InitiateAndSubscribeEvents()
        { }

        protected virtual void UnSubscribeEvents()
        { }
    }
}