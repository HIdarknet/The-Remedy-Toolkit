// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Events/Collision"), Tags("Object")]
    public class OnCollisionExit : SchematicEventNode
    {
        [Output]
        public Collider OtherCollider;
        [Output]
        public Vector3 Impulse;
        [Output]
        public Vector3 RelativeVelocity;
        [Output]
        public Rigidbody OtherRigidbody;
        [Output]
        public ArticulationBody OtherArticulationBody;
        [Output]
        public Transform OtherTransform;
        [Output]
        public GameObject OtherGameObject;
    }
}