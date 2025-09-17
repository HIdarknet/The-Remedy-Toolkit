// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Random"), Tags("Default")]
    public class RandInt : SchematicGraphNode
    {
        [Input("LowerBound", Editable = true)]
        public int LowerBound;
        [Input("UpperBound", Editable = true)]
        public int UpperBound;

        [Output("Value")]
        public int Value;

        public override object OnRequestValue(Port port)
        {
            try
            {
                LowerBound = GetOutputValue<int>("LowerBound");
            }
            catch
            {
                LowerBound = 0;
            }
            try
            {
                UpperBound = GetOutputValue<int>("UpperBound");
            }
            catch 
            {
                UpperBound = 1;
            }

            return UnityEngine.Random.Range(LowerBound, UpperBound);
        }
    }
}