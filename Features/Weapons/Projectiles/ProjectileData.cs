// project armada

#pragma warning disable 0414

/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.Weapons.Projectiles
{
    //[Searchable]
    [CreateAssetMenu(menuName = "Remedy Toolkit/Weapons/Projectile Data")]
    public class ProjectileData : ScriptableObjectWithID<ProjectileData>
    {
        public enum TargetMode
        {
            MatchingTag,
            SpecificInstance
        }
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Targetting")]*/
        public TargetMode TargetingMode;
        [Tooltip("The Tag of an Object that is to be searched for to Target.")]
        public string TargetTag = "";

        [Tooltip("The Distance away from the original shoot position where the projectile lines up completely with the actual aim position.")]
        public float LineUpDistance = 5f;
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Speed")]*/
        public float InitialVelocity;
        [Range(0f, 1f)]
        public float GravityInfluence;


/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Life & Cost")]*/
        public float LifeSpan = 5f;
        [Tooltip("Whether to treat any collision as a valid collision for the Projectile")]
        public bool DestroyOnContactWithAny;
        [Tooltip("The amount of Ammo that is spent to create this projectile")]
        public int AmmoCost;


        [SerializeField]
        [Tooltip("If True, a FixedJoint will be added to the Projectile, and it will be attached to the Weapon that fires it.")]
        private bool _fixToWeapon = false;
        public bool Fixed => _fixToWeapon;

        [Tooltip("The LayerMask to use for Targetting Enemies")]
        //[Layer]
        public int Layer;
        public LayerMask CollisionLayer;
    }
}