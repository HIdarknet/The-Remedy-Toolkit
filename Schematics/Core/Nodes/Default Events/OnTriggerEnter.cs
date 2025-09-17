// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Events/Collision"), Tags("Object")]
    public class OnTriggerEnter : SchematicEventNode
    {
        [Output]
        public Collider collider;
    }
}