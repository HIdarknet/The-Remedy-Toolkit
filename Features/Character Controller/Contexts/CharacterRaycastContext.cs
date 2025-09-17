using Remedy.Framework;
using UnityEngine;

public class CharacterRaycastContext : MonoBehaviour
{
    public bool IsGrounded = false;
    public RaycastUtility.RaycastResult GroundCastResult;
    public RaycastUtility.RaycastResult WallCastResult;
    [SchematicProperties]
    public CharacterRaycastProperties Properties;

    private bool _cached = false;
    private Transform _transform;
    private Rigidbody _rb;

    private void OnEnable()
    {
        if (!_cached)
        {
            _transform = gameObject.GetComponent<Transform>();
            _rb = gameObject.GetCachedComponent<Rigidbody>();
            _cached = true;
        }
    }

    public void PerformGroundCast()
    {
        GroundCastResult = RaycastUtility.SphereCast(_transform.position, Vector3.down, Properties.GroundCheckSphereRadius, Properties.GroundCheckDistance, Properties.CollisionMask, -1f, Properties.MaxGroundSlopeAngle);
        IsGrounded = GroundCastResult.HasHit;
    }
    public void PerformWallCast(Vector3 direction)
    {
        WallCastResult = RaycastUtility.SphereCast(_transform.position, direction, Properties.WallCheckSphereRadius, Properties.WallCheckDistance, Properties.CollisionMask, Properties.MinWallAngle, Properties.MaxWallAngle);
    }
}