using BlueGraph;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Objects"), Tags("Default")] 
    public class Instantiate : SchematicActionNode
    {
        [Input("Position")]
        public Vector3 InitPosition;
        [Input("Rotation")]
        public Vector3 Rotation;
        [Input("Prefab")]
        public GameObject Prefab;

        [Output("Instanced Object")]
        public GameObject InstancedObject;
        
        protected override void OnTrigger(bool awaiting = false)
        {
            if(awaiting)
            {
                //await UniTask.WaitForEndOfFrame();

                InitPosition = GetInputValue<Vector3>("Position");
                Rotation = GetInputValue<Vector3>("Rotation");
                Prefab = GetInputValue<GameObject>("Prefab");

                SetOutputValue(nameof(InstancedObject), ObjectManager.Instantiate(Prefab, InitPosition, Quaternion.Euler(Rotation)));

                return;
            }
            else
            {
                InitPosition = GetInputValue<Vector3>("Position");
                Rotation = GetInputValue<Vector3>("Rotation");
                Prefab = GetInputValue<GameObject>("Prefab");

                SetOutputValue(nameof(InstancedObject), ObjectManager.Instantiate(Prefab, InitPosition, Quaternion.Euler(Rotation)));
            }
        }

        public override object OnRequestValue(Port port)
        {
            if (InstancedObject == null)
                return Prefab;
            return InstancedObject;
        }
    }
}