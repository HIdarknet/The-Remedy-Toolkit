//using SaintsField;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Schematics
{
    public class SchematicInstanceController : MonoBehaviour
    {
        public List<SchematicVariable> variables;

        ///[ReadOnly]
        public GameObject Prefab;
        [Tooltip("Whether the Object is a Singleton or not.")]
        public bool Singleton = false;
        public SchematicGraph[] SchematicGraphs = new SchematicGraph[0];

        protected virtual void OnEnable()
        {
            foreach (var graph in SchematicGraphs)
            {
                graph?.ReconstructPortConnections();
            }
        }

        public void Assign(SchematicGraph graph)
        {
            //graph.GraphController = this;
            if (graph == null) return;

            foreach (var kvp in graph.PathsToChildInstances)
            {
                graph.OriginalToInstantiatedChildren[kvp.Value] = transform.Find(kvp.Key).gameObject;
            }

            foreach(var curGraph in SchematicGraphs)
            {
            }
        }

        public virtual void SetVariable(string name, object value)
        { }

        public virtual object GetVariable(string name)
        { return null; }
    }
}