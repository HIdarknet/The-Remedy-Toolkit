using Remedy.CharacterControllers.WallSlide;
//using SaintsField.Playa;
using UnityEngine;

namespace Remedy.CharacterControllers.Stamina
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/Stamina Properties")]
    public class StaminaProperties : ScriptableObjectWithID<WallSlideControllerProperties>
    {
        [Tooltip("The amount of Stamina on Start")]
        public float MaxStamina;
        [Tooltip("The amount of Stamina regained when replenishing it.")]
        public float StaminaReginAmount = 1f;
    }
}