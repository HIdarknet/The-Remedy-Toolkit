using UnityEngine;
using System.Collections.Generic;
using Remedy.Schematics.Utils;

namespace Remedy.Schematics
{
    [RequireComponent(typeof(ManagerHandshaker))]
    public class SchematicInstanceController : MonoBehaviour
    {
        public List<SchematicVariable> variables;

        ///[ReadOnly]
        public GameObject Prefab;
        [Tooltip("Whether the Object is a Singleton or not.")]
        public bool Singleton = false;
        public SchematicGraph[] SchematicGraphs = new SchematicGraph[0];

        public SchematicScope Scope;

        [ScriptableVariableList]
        public List<ScriptableVariable> Variables = new();

        public bool ServerOnly = false;

        private void OnEnable()
        {
            foreach (var graph in SchematicGraphs)
            {
                graph?.ReconstructPortConnections();

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
                }
            }
        }
        public void Assign(SchematicGraph graph)
        {
            if (graph == null) return;

            foreach (var kvp in graph.PathsToChildInstances)
            {
                graph.OriginalToInstantiatedChildren[kvp.Value] = transform.Find(kvp.Key).gameObject;
            }

            foreach (var curGraph in SchematicGraphs)
            {
            }
        }
    }
}