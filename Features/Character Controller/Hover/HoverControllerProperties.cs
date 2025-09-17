using UnityEngine;
//using SaintsField.Playa;

namespace Remedy.CharacterControllers.Hover
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/3D Platformer/Hover Properties")]
    public class HoverControllerProperties : ScriptableObjectWithID<HoverControllerProperties>
    {
        [Tooltip("The speed at which the input returns to 0 when not being updated." +
            "This is especially important when starting the flight, because the velocity of the previous motion state" +
            "is inherited.")]
        public float InputNormalizationSpeed = 5f;

        [Header("Movement")]
        public float HorizontalSpeed = 10f;

        [Header("Ride and Fall")]
        public float RiseSpeed = 120f;

        public float HoverFallSpeed = 5f;
        [Tooltip("The amount of time to hover in the air before plummeting.")]
        public float HoverTime = 2f;

        [Tooltip("Applied to the Fall Speed over time after Hover Time has been surpassed.")]
        public float PlummetAcceleration = 2.5f;
        [Tooltip("The max fall speed when plummeting")]
        public float PlummetTerminalSpeed = 120f; 

        [Header("Stamina Settings")]
        public float staminaUseRate = 5f;
    }
}