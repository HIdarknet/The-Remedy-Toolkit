
using BlueGraph;
using Remedy.Schematics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    public class SchematicGraphNode : BlueGraph.Node
    {
        public SchematicGraph ScriptGraph => (SchematicGraph)Graph;
        public GameObject GameObject => ScriptGraph.GameObject;
        public Transform Transform => GameObject.transform;

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

        protected Dictionary<string, (SchematicGraphNode node, string portName)[]> _cachedInputValueSources
            = new();

        protected Remedy.Framework.SerializableDictionary<string, Union> _cachedInputsByName = new();
        protected Remedy.Framework.SerializableDictionary<string, Union> _cachedOuputsByName = new();
        protected List<string> _cachedInputNames = new();
        protected List<string> _cachedOutputNames = new();

        protected Remedy.Framework.SerializableDictionary<string, string[]> _cachedPortConnections = new();
        protected List<SchematicActionNode> _cachedChildren = new();

        private List<Union> _returnValList = new();

        public override void OnValidate()
        {
            UpdateCaches();
        }

        /// <summary>
        /// Called while editting the parent graph to cache information about the Node for efficient use during Runtime
        /// </summary>
        public void UpdateCaches()
        {
            _cachedInputsByName.Clear();
            _cachedOuputsByName.Clear();
            _cachedPortConnections.Clear();
            _cachedInputValueSources.Clear();
            _cachedInputNames.Clear();
            _cachedOutputNames.Clear();

            foreach (var kvp in Ports.Where(p => p.Value.Type != typeof(ActionPort)))
            {
                var port = kvp.Value;
                if (port.Direction == PortDirection.Input)
                {
                    _cachedInputsByName[port.Name] = default;
                    _cachedInputNames.Add(port.Name);

                    // Build source mapping
                    var sources = new List<(SchematicGraphNode, string)>();
                    foreach (var connectedPort in port.ConnectedPorts)
                    {
                        if (connectedPort?.Node is SchematicGraphNode sourceNode)
                        {
                            sources.Add((sourceNode, connectedPort.Name));
                        }
                    }
                    _cachedInputValueSources[port.Name] = sources.ToArray();
                }
                else
                {
                    _cachedOuputsByName[port.Name] = default;
                    _cachedOutputNames.Add(port.Name);
                }

                _cachedPortConnections[port.Name] = port.ConnectedPorts.Where(p => p != null).Select(p => p.ID).ToArray();
            }

            RebuildChildrenCache();

            OnCacheUpdate();

            IsDirty = false;
        }

        protected virtual void OnCacheUpdate() { }

        /// <summary>
        /// Rebuilds the cache for the immediate childre of this Graph Node for quick access
        /// </summary>
        protected void RebuildChildrenCache()
        {
            _cachedChildren.Clear();

            foreach (var kvp in Ports)
            {
                var port = kvp.Value;
                if (port.Direction == PortDirection.Output && port.Type == typeof(ActionPort))
                {
                    _cachedChildren.Capacity = port.ConnectedPorts.Count();
                    // Iterate directly instead of ToList()
                    foreach (var connectedPort in port.ConnectedPorts)
                    {
                        if (connectedPort?.Node is SchematicActionNode child)
                            if(!_cachedChildren.Contains(child))
                                _cachedChildren.Add(child);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value of the Union for the port with the given name to the given value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetOutputValue(string name, Union value)
        {
            if (IsDirty)
                UpdateCaches();

            _cachedOuputsByName[name] = value;
        }


        /// <summary>
        /// Returns the value of the Union for the Given Port cast to the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public new T GetInputValue<T>(string name, T defaultValue = default)
        {
            /*            if(HasInputConnections(name))
                        {*/
            if(IsDirty)
                UpdateCaches();

            try
            {
                PullMostRecentInputValue(name);
                var value = _cachedInputsByName[name].Get<T>();
               return value;
            }
            catch
            {
                return defaultValue;
            }
            //}
        }

        /// <summary>
        /// Returns the value of the Union for the Given Port cast to the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public new List<T> GetInputValues<T>(string name)
        {
            if (IsDirty)
                UpdateCaches();

            //UpdateCaches();
            PullAllInputValues(name);
            return _cachedInputsByName[name].Get<List<T>>();
        }

        /// <summary>
        /// Returns true if the input port with the given name has any connected sources.
        /// </summary>
        public bool HasInputConnections(string inputName)
        {
            if (IsDirty)
                UpdateCaches();

            //UpdateCaches(); // make sure the cache is up-to-date
            if (!_cachedInputValueSources.TryGetValue(inputName, out var sources))
                return false;

            return sources != null && sources.Length > 0;
        }


        /// <summary>
        /// Returns the value of the Union for the Given Port cast to the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public new T GetOutputValue<T>(string name)
        {
            if (IsDirty)
                UpdateCaches();

            return _cachedOuputsByName[name].Get<T>();
        }

        /// <summary>
        /// Chooses the most recently updated Union Value as the one to use for the given Port.
        /// Most common input pull, as the most recent is usually expected.
        /// </summary>
        /// <param name="inputName"></param>
        private void PullMostRecentInputValue(string inputName)
        {
            (SchematicGraphNode node, string name) mostRecentPort = (null, null);
            int mostRecentTick = 0;

            var sources = _cachedInputValueSources[inputName];

            for(int i = 0; i < sources.Length; i++)
            {
                var (node, portName) = sources[i];

                if (node._cachedOuputsByName[portName].LastUpdateTick > mostRecentTick)
                {
                    mostRecentPort = (node, portName);
                    mostRecentTick = node._cachedOuputsByName[portName].LastUpdateTick;
                }
            }

            if(mostRecentPort.node != null)
                _cachedInputsByName[inputName] = mostRecentPort.node._cachedOuputsByName[mostRecentPort.name];
        }

        /// <summary>
        /// Sets the value of the given Input to a Union List of all the combined values connected to the Port.
        /// Good when you need to get multiple values for a Node.
        /// </summary>
        /// <param name="inputName"></param>
        private void PullAllInputValues(string inputName)
        {
            _returnValList.Clear();

            foreach(var kvp in _cachedInputValueSources)
            {
                Debug.Log(kvp.Key + ": " + kvp.Value);
            }

            var sources = _cachedInputValueSources[inputName];

            for (int i = 0; i < sources.Length; i++)
            {
                var (node, portName) = sources[i];

                _returnValList.Add(node._cachedOuputsByName[portName]);
            }

            _cachedInputsByName[inputName] = _returnValList;
        }

        public override object OnRequestValue(BlueGraph.Port port)
        {
            return null;
        }
    }
}