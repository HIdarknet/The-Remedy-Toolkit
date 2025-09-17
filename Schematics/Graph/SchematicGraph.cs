using BlueGraph;
using Remedy.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using SaintsField;
using System.Reflection;
using Remedy.Schematics.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Remedy.Schematics
{
    public class SchematicGraph : Graph
    {
        public override string Title
        {
            get { return Prefab == null ? "SCHEMATIC" : Prefab.name; }
        }

        [SerializeField]
        private GameObject _prefab;
        public GameObject Prefab
        {
            get
            {
                return _prefab;
            }
            set
            {
                if(Scope != null)
                    Scope.Prefab = value;

                _prefab = value;
            }
        }
        public SchematicScope Scope;

        [SerializeField]
        [TextArea(1, 3)]
        private string _description;
        
        //public List<ScriptGraphPropertySerializer> Parameters = new();

        //[Dropdown("GetParameterTypes")]
        //public string ParameterTypeToAdd;

        [HideInInspector]
        public bool Returned;
        public object ReturnValue;

        [HideInInspector]
        [SerializeField]
        private SerializableDictionary<SerializableType, SchematicEventNode[]> _eventNodesByType;
        public SerializableDictionary<SerializableType, SchematicEventNode[]> EventNodesByType => _eventNodesByType ??= new(); 
        [HideInInspector]
        [SerializeField]
        private FlowInvokeBase[] _flowInvokeNodesCache = new FlowInvokeBase[0];
        public FlowInvokeBase[] FlowInvokeNodesCache => _flowInvokeNodesCache;
        [HideInInspector]
        [SerializeField]
        private FlowOnInvokeBase[] _flowOnInvokeNodesCache = new FlowOnInvokeBase[0];
        public FlowOnInvokeBase[] FlowOnInvokeCache => _flowOnInvokeNodesCache;

//        [ReadOnly]
        [Tooltip("A Dictionary pairing the Prefab's original Children to the Instantiated copies of them.")]
        public SerializableDictionary<UnityEngine.Object, UnityEngine.Object> OriginalToInstantiatedChildren = new();
//        [ReadOnly]
        [Tooltip("A Dictionary pairing the Prefab's original Children's paths to their original references.")]
        public Dictionary<string, GameObject> PathsToChildInstances = new();
/*
        public DropdownList<string> GetParameterTypes()
        {
            DropdownList<string> list = new();

            var parameterTypes = typeof(ScriptGraphParameter).GetInheritedTypes().Where(type => type.GenericTypeArguments.Length == 0).ToList();

            if (parameterTypes.Count == 0)
                list.Add("No Parameter Types", "");
            else
            {
                foreach (var paramType in parameterTypes)
                {
                    list.Add(paramType.Name, paramType.AssemblyQualifiedName);
                }
            }

            return list;
        }
*/
        private GameObject _gameObject;

        /// <summary>
        /// The Game Object that this Script Graph is attached to. This can only be set once, and should be done by the <seealso cref="SchematicInstanceController"/> 
        /// </summary>
        public GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
            }
        }
/*        [SerializeField]
        public SchematicInstanceController GraphController;*/

        private Rigidbody _rigidBody;
        public Rigidbody RigidBody
        {
            get
            {
                if (_rigidBody == null)
                    _rigidBody = GameObject.GetComponent<Rigidbody>();
                return _rigidBody;
            }
        }


        /// <summary>
        /// Get the given Event Node from the Graph
        /// </summary>
        /// <typeparam name="TEventNode"></typeparam>
        /// <returns></returns>
        public SchematicEventNode[] GetEventArray<TEventNode>() where TEventNode : SchematicEventNode
        {
            return _eventNodesByType[typeof(TEventNode)];
        }

        /// <summary>
        /// Trigger all the Events of the given type present in the Graph
        /// </summary>
        /// <typeparam name="TEventNode"></typeparam>
        public void TriggerEvent<TEventNode>(params Union[] args) where TEventNode : SchematicEventNode
        {
            var eventArr = GetEventArray<TEventNode>();

            if (eventArr != null && eventArr.Length > 0)
                foreach (var node in eventArr) { node.Trigger(args); }
        }

        protected override void ResetExtendedNodeCaches()
        {
            EventNodesByType.Clear();
            _flowInvokeNodesCache = new FlowInvokeBase[0];
            _flowOnInvokeNodesCache = new FlowOnInvokeBase[0];
        }

        protected override void CacheNodeByType(Node node, CacheToAttribute attr)
        {
            base.CacheNodeByType(node, attr);
            
            if(attr != null)
            {
                if (typeof(SchematicEventNode).IsAssignableFrom(node.GetType()))
                {
                    var eventNode = (SchematicEventNode)node;
                    if (!EventNodesByType.ContainsKey(attr.Type))
                        EventNodesByType.Add(attr.Type, new SchematicEventNode[0]);
                    EventNodesByType[attr.Type] = EventNodesByType[attr.Type].Append(eventNode).ToArray();
                }
            }
            if (attr == null || attr.CacheAsBoth)
            {
                var type = node.GetType();

                if (typeof(SchematicEventNode).IsAssignableFrom(type))
                {
                    var eventNode = (SchematicEventNode)node;
                    if (!EventNodesByType.ContainsKey(type) || EventNodesByType[type] == null)
                        EventNodesByType[type] = new SchematicEventNode[0];
                    EventNodesByType[type] = EventNodesByType[type].Append(eventNode).ToArray();
                }

                if (typeof(FlowInvokeBase).IsAssignableFrom(type))
                {
                    if (!_flowInvokeNodesCache.Contains(node))
                        _flowInvokeNodesCache = _flowInvokeNodesCache.Append((FlowInvokeBase)node).ToArray();

                    var invokeNode = (FlowInvokeBase)node;
                    invokeNode.UpdateCaches();
                }
                if (typeof(FlowOnInvokeBase).IsAssignableFrom(type))
                {
                    if (!_flowOnInvokeNodesCache.Contains(node))
                        _flowOnInvokeNodesCache = _flowOnInvokeNodesCache.Append((FlowOnInvokeBase)node).ToArray();

                    var invokeNode = (FlowOnInvokeBase)node;
                    invokeNode.UpdateCaches();
                }
            }
        }
    }

    public enum ComparisonOperator
    {
        Equal,
        GreaterThan,
        LessThan,
        EqualOrGreaterThan,
        EqualOrLessThan
    }

    [Serializable]
    public class ScriptGraphPropertySerializer
    {
        public string Name;
        //[Expandable]
        public ScriptGraphParameter reference;
    }

    /// <summary>
    /// A helper class for Instances of Node Connections so that the Graph can resolve connections if they're broken.
    /// </summary>
    [Serializable]
    public class ConnectionInfo
    {
        public Port Port;
        public List<Connection> Connections = new();

        public ConnectionInfo(Port port)
        {
            Port = port;
        }

        /// <summary>
        /// Iterates through the Ports that were meant to be connected and reconnects them if they're not
        /// </summary>
        public void Rewire()
        {
            Port.Connections = Connections;
        }
    }
}
