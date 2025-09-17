// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = ""), Tags("Object")]
    public class This : SchematicGraphNode
    {
        [Output(" ")]
        public object Value;

        public override object OnRequestValue(Port port)
        {
            return GameObject;
        }
    }
}