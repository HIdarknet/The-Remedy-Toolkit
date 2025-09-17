/*using SaintsField;
using SaintsField.Playa;*/
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Weapons
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObjectWithID<WeaponData>
    {
        [Tooltip("The Weapon Prefab, linked by it's Weapon Behaviour")]
        public Weapon Prefab;
        [Tooltip("Different Fire Modes allows the Weapon to fire different projectiles and function independently.")]
        public FireMode[] FireModes;
        [Tooltip("The amount of time it takes to switch to this Weapon from something else")]
        public float EquipSpeed = 1f;
        [Tooltip("The amount of time it takes to switch away from this Weapon")]
        public float UnequipSpeed = 1f;
    }
}