//using SaintsField;
using UnityEngine;

namespace Remedy.Cameras
{
    [RequireComponent(typeof(BoxCollider))]
    public class CameraPropertiesVolume : MonoBehaviour
    {
        public bool ChangeZoom = false;
        //[ShowIf("ChangeZoom")]
        public float Zoom = 15f;
        public bool ChangeFOV = false;
        //[ShowIf("ChangeFOV")]
        public float FOV = 60f;
        public bool ChangeRestingAngle = false;
        //[ShowIf("ChangeRestingAngle")]
        public float RestingAngle = 60f;
        public bool UseReferenceCam = false;
        //[ShowIf("UseReferenceCam")]
        public Transform ReferenceCamera;
        //[ShowIf("UseReferenceCam")]
        [Tooltip("A mask to applied to copying the position of the Reference Camera")]
        public Vector3Bool ReferenceCamPositionMask = Vector3Bool.False;
        //[ShowIf("UseReferenceCam")]
        [Tooltip("A mask applied to euler angle copying of the reference Camera")]
        public Vector3Bool ReferenceCamAngleMask = Vector3Bool.False;
        //[ShowIf("UseReferenceCam")]
        [Tooltip("The Speed at which the Player's Camera is pulled to the Reference Camera")]
        public float ReferenceCameraSpeed;

        private void OnValidate()
        {
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
        }
    }
}