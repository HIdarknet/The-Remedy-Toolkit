/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.Damagables
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/Damagables/Damage Instigation Data")]
    public class DamageInstigationData : ScriptableObjectWithID<DamageInstigationData>
    {
        public enum InstigationType
        {
            OnCollisionEnter,
            OnTriggerStay,
            OnTriggerEnter,
            External
        }
        public InstigationType Instigation;

        //[Dropdown("GetDamageLayers")]
        public int DamageLayer;
        //public DropdownList<int> GetDamagelayers => DamageData.GetDamageLayers();
        
        //[Dropdown("GetDamageTypes")]
        [Tooltip("The Damage Type, used to modify the way Damage is inflicted to Damagables")]
        public DamageType DamageType;
        [Tooltip("The amount of Damage to be inflicted, handled differently for each IDamagable")]
        public float Amount;
        [Tooltip("The duration of time the Damage is infliced")]
        public float Duration;
        public float Radius;
/*
        public DropdownList<DamageType> GetDamageTypes()
        {
            DropdownList<DamageType> list = new();
            var damageTypes = Resources.LoadAll<DamageType>("");
            if (damageTypes.Length == 0)
                list.Add("No Damage Types", null);
            else
            {
                foreach (var damageType in damageTypes)
                {
                    list.Add(damageType.name, damageType);
                }
            }
            return list;
        }*/
    }
}