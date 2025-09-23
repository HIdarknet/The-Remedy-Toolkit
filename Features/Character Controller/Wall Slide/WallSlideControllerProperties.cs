using UnityEngine;

namespace Remedy.CharacterControllers.WallSlide
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/3D Platformer/Wall Slide & Jump Properties")]
    public class WallSlideControllerProperties : ScriptableObjectWithID<WallSlideControllerProperties>
    {
        public float GapFromWall = 0.5f;

        [Header("Jump")]
        public float JumpVerticalForce = 3f;
        public float JumpHorizontalForce = 2f;
        public float JumpLateralInputInfluence = 0.25f;

        [Header("Slide")]
        public float WallSlideSpeed = 2f;
    }
}