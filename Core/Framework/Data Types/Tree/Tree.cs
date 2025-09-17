//Script Developed for The Remedy Engine, by Richy Mackro (Chad Wolfe), on behalf of Remedy Creative Studios

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Remedy.Framework
{
    [Serializable]
    public class Tree<T> : ISerializationCallbackReceiver
    {
        public UnityEvent<Node<T>> OnItemAdded = new();
        public UnityEvent<Node<T>> OnItemRemoved = new();

        /// <summary>
        /// The Root of the Tree
        /// </summary>
        public Node<T> RootNode;
        /// <summary>
        /// The Currently Selected Node of the Tree
        /// </summary>
        [NonSerialized] public Node<T> CurrentNode = null;

        public int Count
        {
            get
            {
                return Flatten().Length;
            }
        }

        public string CurrentValue;
        /// <summary>
        /// The Cursor Placement in the Tree
        /// </summary>
        [NonSerialized] public int[] Cursor;
        /// <summary>
        /// Gets and Sets the Value at the given Index
        /// </summary>
        /// <param name="index">Index.</param>
        public T this[params int[] index]
        {
            get
            {
                bool searchedRoot = false;

                Node<T> curNode = null;

                foreach (int token in index)
                {
                    if (!searchedRoot)
                        curNode = this.RootNode.GetNode(token);
                    else
                        if (curNode != null)
                        curNode = curNode.GetNode(token);

                    searchedRoot = true;
                }

                if (curNode != null)
                {
                    return curNode.value;
                }

                return default;
            }
            set
            {
                bool searchedRoot = false;

                Node<T> curNode = null;

                foreach (int token in index)
                {
                    if (!searchedRoot)
                        curNode = this.RootNode.GetNode(token);
                    else
                        if (curNode != null)
                        curNode = curNode.GetNode(token);

                    searchedRoot = true;
                }

                if (curNode != null)
                {
                    curNode.value = value;
                }
            }
        }

        /// <summary>
        /// Gets the Node in the Tree with the given Value
        /// </summary>
        /// <param name="nodeValue">Node value.</param>
        public Node<T> this[T nodeValue]
        {
            get
            {
                Node<T>[] nodeArray = Flatten();

                foreach (Node<T> node in nodeArray)
                {
                    if ((object)node.value == (object)nodeValue)
                    {
                        return node;
                    }
                }

                return null;
            }
        }

        public Tree(T initialValue = default)
        {
            this.RootNode = new Node<T>(this, initialValue, new int[] { 0 });
            this.CurrentNode = this.RootNode;
        }

        //todo: Finish Operation Methods
        public bool StepBack()
        {
            if (this.CurrentNode.parent != null)
            {
                this.CurrentNode = this.CurrentNode.parent;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Steps the Tree forward into the given Node
        /// </summary>
        public bool StepForward(int node)
        {
            if (this.CurrentNode.children == null) return false;
            if (this.CurrentNode.children.Count > node)
            {
                this.CurrentNode = this.CurrentNode.children[node];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Step Forward to the Node with the Given Value
        /// </summary>
        /// <param name="value">Value.</param>
        public bool StepForward(T value)
        {
            Node<T> lastNode = this.CurrentNode;

            if (lastNode.children == null) return false;
            foreach (Node<T> node in lastNode.children)
            {
                if ((object)node.value == (object)value)
                {
                    this.CurrentNode = node;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Step Forward to the Node with the Given String Value
        /// </summary>
        /// <param name="valueToString">Value to string.</param>
        public bool StepForward(string valueToString)
        {
            Node<T> lastNode = this.CurrentNode;

            if (lastNode.children == null) return false;
            foreach (Node<T> node in lastNode.children)
            {
                if (node.value.ToString() == valueToString)
                {
                    this.CurrentNode = node;
                    return true;
                }
            }

            return false;
        }


        public void Reset()
        {
            this.CurrentNode = RootNode;
        }

        /// <summary>
        /// Attaches one Node to a Node at the given Index
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="index">Index.</param>
        public void Add(Node<T> node, int[] index)
        {
            bool searchedRoot = false;

            Node<T> curNode = null;

            foreach (int token in index)
            {
                if (!searchedRoot)
                {
                    curNode = RootNode;
                }
                else
                {
                    if (curNode.children == null) return;

                    if (curNode.children.Count > token)
                        curNode = curNode.GetNode(token);
                }

                searchedRoot = true;
            }

            curNode.Add(node);
        }

        /// <summary>
        /// Attaches one Node to a Node at the given Index
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="index">Index.</param>
        public void Add(T node, int[] index)
        {
            bool searchedRoot = false;

            Node<T> curNode = null;

            foreach (int token in index)
            {
                if (!searchedRoot)
                {
                    curNode = RootNode;
                }
                else
                {
                    if (curNode.children == null) return;
                    if (curNode.children.Count > token) curNode = curNode.GetNode(token);
                }

                searchedRoot = true;
            }

            curNode.Add(new Node<T>(this, node, index));
        }

        /// <summary>
        /// Adds the Value to the Tree, as a Child to the given Parent.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parent"></param>
        public void Add(T value, T parent)
        {
            var parentNode = this[parent];
            _ = parentNode == null ? _ = RootNode : parentNode;
            this[parent].Add(new Node<T>(this, value, new int[0]));
        }

        /// <summary>
        /// Searches recursively through the Tree until the Value is found, and removes the give Node. 
        /// Trimming this Node will remove all of it's children as well.
        /// </summary>
        /// <param name="value"></param>
        public void Remove(T value)
        {
            RecursiveRemovalSearch(RootNode, value);
        }

        private void RecursiveRemovalSearch(Node<T> node, T value)
        {
            foreach (var child in node.children)
            {
                if (child.value.Equals(value))
                {
                    node.children.Remove(child);
                    return;
                }
                else
                {
                    RecursiveRemovalSearch(child, value);
                }
            }
        }

        /// <summary>
        /// Gets all the Nodes in the Tree as an Array
        /// </summary>
        /// <returns>The array.</returns>
        public Node<T>[] Flatten()
        {
            List<Node<T>> asList = new List<Node<T>>();
            AddChildren(asList, RootNode);
            return asList.ToArray();
        }

        /// <summary>
        /// Returns the Tree as a JSON Object
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:RemedyData.Tree`1"/>.</returns>
        public override string ToString()
        {
            CurrentValue = CurrentNode.Value;
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Adds the Children of the Node to the List
        /// </summary>
        /// <param name="list">List.</param>
        /// <param name="node">Node.</param>
        private void AddChildren(List<Node<T>> list, Node<T> node)
        {
            if (node.children != null)
            {
                foreach (Node<T> child in node.children)
                {
                    list.Add(child);
                }
                foreach (Node<T> child in node.children)
                {
                    AddChildren(list, child);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            if (this.CurrentNode != null)
            {
                CurrentValue = CurrentNode.Value;
                return;
            }
        }

        public void OnAfterDeserialize()
        {
            if (this.CurrentNode != null)
            {
                CurrentValue = CurrentNode.Value;
                return;
            }
        }
    }
}