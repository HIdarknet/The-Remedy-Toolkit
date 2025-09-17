// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Objects"), Tags("Default")]
    public class FindGameObjectsWithTag : SchematicGraphNode
    {
        [Editable]
        public string Tag;

        [Output("Objects")]
        public List<object> Objects;

        public override object OnRequestValue(Port port)
        {
            return GameObject.FindGameObjectsWithTag(Tag).ToList<object>();
        }
    }
}