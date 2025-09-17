// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    public class Cast<T> : SchematicGraphNode
    {
        [Input("Value")]
        public object Value;

        [Output("Casted")]
        public T CastedValue;

        public override object OnRequestValue(Port port)
        {
            CastedValue = (T)GetOutputValue<object>("Value");
            return CastedValue;
        }
    }
}