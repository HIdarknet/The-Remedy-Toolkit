// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Conditions"), Tags("Default")]
    public class IsNull : SchematicConditionalNode
    {
        [Input("Value")]
        public object value;

        public override bool Condition()
        {
            try
            {
                return GetInputValue<object>("Value") == null;
            }
            catch(Exception e)
            {
                return true;
            }
        }
    }
}