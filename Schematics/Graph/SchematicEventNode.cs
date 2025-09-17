using UnityEngine;
using BlueGraph;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Remedy.Schematics.Utils;

namespace Remedy.Schematics
{
    [Serializable]
    [Output("▶", typeof(ActionPort), Multiple = true)]
    public class SchematicEventNode : SchematicGraphNode
    {
        protected bool _blocked = false;


        public override object OnRequestValue(Port port)
        {
            return null;
        }

        public void Trigger(params Union[] arguments)
        {
            int i = 0;
            foreach(var kvp in _cachedOutputNames)
            {
                _cachedOuputsByName[kvp] = arguments[i];

                i++;
            }

            OnTrigger();
            ProcessChildren();
        }

        protected virtual void OnTrigger()
        {
        }

        public void ProcessChildren(bool awaiting = false, bool parallel = true)
        {
            if (IsDirty)
                UpdateCaches();

            if (_cachedChildren.Count == 0) return;

                for (int i = 0; i < _cachedChildren.Count; i++)
                {
                    var child = _cachedChildren[i];
                    child.Trigger(false);
                }
        }
    }
}
