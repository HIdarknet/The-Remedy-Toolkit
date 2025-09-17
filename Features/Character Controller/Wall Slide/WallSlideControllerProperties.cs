//using SaintsField.Playa;
using UnityEngine;

namespace Remedy.CharacterControllers.WallSlide
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/3D Platformer/Wall Slide & Jump Properties")]
    public class WallSlideControllerProperties : ScriptableObjectWithID<WallSlideControllerProperties>
    {
        public float AlignmentSpeed = 15f;
        public float WallFriction = 10f;

        [Header("Jump")]
        public float JumpForce = 5f;
        public float JumpOffForce = 10f;
        public int JumpForceDuration = 60;
        public float CancelJumpGravity = 20f;        // Gravity override when jump is canceled early
        public int OrientationTime = 30;

        [Header("Slide")]
        public float WallSlideSpeed = 2f;
        public float WallStickForce = 1f; // New: Force to pull character into wall
    }
}