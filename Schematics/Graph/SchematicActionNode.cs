using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Output("▶", typeof(ActionPort), Multiple = true)]
    public abstract class SchematicActionNode : SchematicActionNodeBase
    { }

    [Serializable]
    [Input("▷", typeof(ActionPort), Multiple = true)]
    public abstract class SchematicActionNodeBase : SchematicGraphNode
    {
        protected bool _processChildren = true;

        private string _errorMessage;
        public string ErrorMessage => _errorMessage;

        public override object OnRequestValue(Port port)
        {
            return null;
        }

        public void Trigger(bool awaiting = false)
        {
            OnTrigger(false);

            if (_processChildren)
            {
                ProcessChildren(false);
            }
        }

        protected virtual void OnTrigger(bool awaiting = false)
        {
        }

        public void ProcessChildren(bool awaiting = false, bool parallel = false)
        {
            if (IsDirty)
                UpdateCaches();

            if (_cachedChildren.Count == 0) return;

            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                var child = _cachedChildren[i];
                child.Trigger(awaiting);
            }
        }
    }
}
