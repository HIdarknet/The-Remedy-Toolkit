using UnityEngine;

namespace Remedy.Cameras
{
    [CreateAssetMenu(menuName = "Remedy Toolkit/Camera/Operator Properties")]
    public class CameraOperatorProperties : ScriptableObject
    {
        [Tooltip("Time it takes to transition to the Operator from the last one")]
        public float TransitionTime = 2f;
        public float XAxisSpeed = 3f;
        public float YAxisSpeed = 2f;
        public float SmoothSpeed = 10f;
        [Tooltip("If True, the Camera Manager will follow refer to this Operator's Current Volume for Camera Properties.")]
        public bool UseVolumes = false;
    }
}