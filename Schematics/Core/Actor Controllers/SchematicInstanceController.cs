using UnityEngine;
using System.Collections.Generic;
using Remedy.Schematics.Utils;

namespace Remedy.Schematics
{
    [RequireComponent(typeof(ManagerHandshaker))]
    public class SchematicInstanceController<T> : SchematicInstanceController where T : SchematicGraph
    {
        public SchematicScope Scope;

        private Dictionary<string, object> _variables = new();

        public bool ServerOnly = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            foreach(var graph in SchematicGraphs)
            {
                //graph.Prefab = gameObject;
                Assign(graph);

                foreach (var oninvokeNode in graph.FlowOnInvokeCache)
                {
                    var isInvoking = false;

                    oninvokeNode.Subscription.instance = this;
                    oninvokeNode.Subscription.action = (Union value) =>
                    {
                        if (isInvoking) return;
                        try
                        {
                            isInvoking = true;
                            oninvokeNode?.Trigger(value);
                        }
                        finally
                        {
                            isInvoking = false;
                        }
                    };

                    oninvokeNode.UpdateCaches();
                }
            }
        }

        private void Start()
        {
            foreach(var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnCreate>();
                }
            }
        }

        private void Update()
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnUpdate>();
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnDestroy>();
                }
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnCollisionEnter>(collision.collider, collision.impulse, collision.relativeVelocity, collision.articulationBody, collision.transform, collision.gameObject);
                }
            }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnCollisionExit>(collision.collider, collision.impulse, collision.relativeVelocity, collision.articulationBody, collision.transform, collision.gameObject);
                }
            }
        }
        protected virtual void OnTriggerEnter(Collider collider)
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnCollisionEnter>(collider);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    ScriptGraph?.TriggerEvent<OnCollisionExit>(collider);
                }
            }
        }

        public void OnValidate()
        {
            foreach (var ScriptGraph in SchematicGraphs)
            {
                if (ScriptGraph != null)
                {
                    ScriptGraph.GameObject = gameObject;
                    //ScriptGraph.Prefab = gameObject;
                }
            }
        }
        
        public override void SetVariable(string name, object value)
        {
            if(!_variables.ContainsKey(name))
                _variables.Add(name, value);
            _variables[name] = value;
        }

        public override object GetVariable(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];
            return null;
        }
    }
}