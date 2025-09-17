// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;


namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Events/GameObject"), Tags("Object")]
    public class OnUpdate : SchematicEventNode
    {
        protected override void OnTrigger()
        {
            base.OnTrigger();
        }
    }
}