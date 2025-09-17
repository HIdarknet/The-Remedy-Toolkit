// project armada

#pragma warning disable 0414

using BlueGraph;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Collections"), Tags("Default")]
    public class CreateList : SchematicGraphNode
    {
        [Input("Items", Multiple = true)]
        public object Items;

        [Output(" ")]
        public List<object> NewCollection;

        public override object OnRequestValue(Port port)
        {
            var itemList = GetInputValues<object>("Items").ToList();
            itemList ??= new List<object>();

            var newList = new List<object>();

            foreach(var item in itemList)
            {
                if(item is IEnumerable && !(item is string))
                {
                    newList.AddRange(((IEnumerable<object>)item).ToList());
                }
                else
                {
                    newList.Add(item);
                }
            }

            return newList;
        }
    }
}