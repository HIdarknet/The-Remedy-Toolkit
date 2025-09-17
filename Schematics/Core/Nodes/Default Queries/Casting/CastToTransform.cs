// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Casts"), Tags("Default")]
    public class CastToTransform : Cast<Transform>
    {
        [Output("Position")]
        public Vector3 TransformPosition;
        [Output("EulerAngles")]
        public Vector3 EulerAngles;
        public override object OnRequestValue(Port port)
        {
            var value = GetOutputValue<object>("Value");

            if (value.GetType() == typeof(Transform))
                CastedValue = (Transform)value;
            if (value.GetType() == typeof(GameObject))
                CastedValue = ((GameObject)value).transform;
            if (value.GetType() == typeof(Component))
                CastedValue = ((Component)value).transform;
            if (value.GetType() == typeof(MonoBehaviour))
                CastedValue = ((MonoBehaviour)value).transform;

            if (port.Name == "Casted")
            {
                return CastedValue;
            }
            else if (port.Name == "Position")
            {
                return CastedValue.position;
            }
            else if (port.Name == "EulerAngles")
            {
                return CastedValue.eulerAngles;
            }
            return null;
        }
    }
}