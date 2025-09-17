/*using SaintsField;
using SaintsField.Playa;*/
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.Damagables
{
    //[Searchable]
    [CreateAssetMenu(fileName = "DamageType", menuName = "Remedy Toolkit/Damagables/Damage Type")]
    public class DamageType : ScriptableObject
    {
        //[SaintsDictionary]
        [Tooltip("Names of Buffs, with Multipliers that change the way this DamageType affects Health or Damage to the Player")]
        public Dictionary<string, float> BuffMultipliers = new();
    }
}