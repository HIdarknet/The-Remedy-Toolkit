// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Collections"), Tags("Default")]
    public class ListCount : SchematicGraphNode
    {
        [Input("List")]
        public List<object> List;

        [Output("Count")]
        public int Count;

        public override object OnRequestValue(Port port)
        {
            List = GetOutputValue<List<object>>("List");

            return List.Count;
        }
    }
}