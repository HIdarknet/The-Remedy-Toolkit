using Remedy.Framework;
/*using SaintsField;
using SaintsField.Playa;*/
using System.Collections.Generic;
using UnityEngine;
using Remedy.Common;

namespace Remedy.Damagables
{
    //[Searchable]
    public class DamageManager : Singleton<DamageManager>
    {
        //[ReadOnly, SaintsDictionary]
        private Dictionary<int, List<Damageable>> _activeDamageables = new();
        public static Dictionary<int, List<Damageable>> ActiveDamageables => Instance._activeDamageables;

        private Dictionary<GameObject, int> _damageables;
        public static Dictionary<GameObject, int> Damageables => Instance._damageables ??= new();

        private void OnEnable()
        {
            for(int i = 0; i < DamageData.Instance.DamageLayers.Length; i++)
            {
                ActiveDamageables.Add(i, new());
            }
        }

        public static void AddDamageable(Damageable damageable)
        {
            ActiveDamageables[damageable.DamageLayer].Add(damageable);
            Damageables.Add(damageable.gameObject, damageable.DamageLayer);
        }

        public static void RemoveDamageable(Damageable damageable)
        {
            ActiveDamageables[damageable.DamageLayer].Remove(damageable);
            Damageables.Remove(damageable.gameObject);
        }

        public static void InstigateDamage(DamageInstigation instigation)
        {
            var data = DamageInstigationData.Lookup[instigation.DataID];
            var damageables = ActiveDamageables[data.DamageLayer];

            foreach (var damageable in damageables)
            {
                if (Vector3.Distance(damageable.transform.position, instigation.Position) < data.Radius)
                {
                    damageable.Damage(instigation);
                }
            }
        }

        public void InstigateHeal(DamageInstigation instigation)
        {
            var data = DamageInstigationData.Lookup[instigation.DataID];
            var damageables = ActiveDamageables[data.DamageLayer];

            foreach (var damageable in damageables)
            {
                if (Vector3.Distance(damageable.transform.position, instigation.Position) < data.Radius)
                {
                    damageable.Damage(instigation);
                }
            }
        }
    }
}