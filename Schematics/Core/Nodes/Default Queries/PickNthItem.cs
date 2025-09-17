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
    public class PickNthItem : SchematicGraphNode
    {
        [Input("List")]
        public List<object> List;
        [Input("Index")]
        public int Index;

        [Output("Item")]
        public object Item;

        public override object OnRequestValue(Port port)
        {
            List = GetOutputValue<List<object>>("List");
            Index = GetOutputValue<int>("Index");

            return List[Index];
        }
    }
}