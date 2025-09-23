using UnityEngine;

namespace Remedy.CharacterControllers.LedgeGrab
{
    [CreateAssetMenu(menuName = "Remedy Toolkit/3D Platformer/Ledge Grab Properties")]
    public class LedgeGrabControllerProperties : ScriptableObjectWithID<LedgeGrabControllerProperties>
    {
        [Header("Ledge Adjustment")]
        public float LedgeCheckUp = 1f;
        public float LedgeCheckForward = 0.5f;
        public float LedgeCheckRadius = 0.5f;

        public float ClamberHeight = 1f;
        public float ClamberForward = 1f;

        public float GapFromWall = 0.5f;
    }
}