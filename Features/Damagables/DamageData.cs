using Remedy.Framework;
//using SaintsField;
using UnityEngine;

namespace Remedy.Damagables
{
    public class DamageData : SingletonData<DamageData>
    {
        [Tooltip("The layers that Damage Instigators use to filter out Damageables that are affected by them.")]
        public string[] DamageLayers;
/*
        public static DropdownList<int> GetDamageLayers()
        {
            var list = new DropdownList<int>();

            for (int i = 0; i < DamageData.Instance.DamageLayers.Length; i++)
            {
                list.Add(DamageData.Instance.DamageLayers[i], i);
            }

            return list;
        }*/
    }
}