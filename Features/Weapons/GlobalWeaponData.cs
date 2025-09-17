using Remedy.Framework;
using System.Collections.Generic;
using UnityEngine;
//using SaintsField;

namespace Remedy.Weapons
{
    [CreateAssetMenu(menuName = "Remedy Toolkit/Weapons/Global Weapon Data")]
    public class GlobalWeaponData : SingletonData<GlobalWeaponData>
    {
        [Tooltip("The Collision Layer used for interacting with Weapons in the world (pickup/drop)")]
        //[Layer]
        public int WeaponCollisionLayer;

        [Tooltip("Weapon Types determine what kind of Object can hold a particular Weapon.")]
        public List<string> WeaponTypes = new()
    {
        "Player",
        "Vehicle"
    };

        public bool SwitchOnPickup = false;
    }
}