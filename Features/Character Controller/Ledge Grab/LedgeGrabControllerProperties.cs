//using SaintsField.Playa;
using UnityEngine;

namespace Remedy.CharacterControllers.LedgeGrab
{
    [CreateAssetMenu(menuName = "Remedy Toolkit/3D Platformer/Ledge Grab Properties")]
    //[Searchable]
    public class LedgeGrabControllerProperties : ScriptableObjectWithID<LedgeGrabControllerProperties>
    {

        [Header("Ledge Adjustment")]
        public float LedgeOffset = 1f;
        public float LedgeCheckRadius = 0.25f;

        public LayerMask WallLayer;

        public float MaxStickAngle = 100f;
        public float AlignmentSpeed = 15f;

        [Header("Jump")]
        public float JumpForce = 5f;
        public float JumpOffForce = 10f;
        public int JumpForceDuration = 60;
        public int OrientationTime = 30;

        [Header("Clamber")]
        public int ClamberTime = 60;
    }
}