using UnityEngine;

public class CharacterRaycastProperties : ScriptableObjectWithID<CharacterRaycastProperties>
{
    public LayerMask CollisionMask = ~0;
    public float GroundCheckDistance = 3f;
    public float WallCheckDistance = 0.25f;
    public float GroundCheckSphereRadius = 0.5f;
    public float WallCheckSphereRadius = 0.5f;
    public float MaxGroundSlopeAngle = 80;
    public float MinWallAngle = 30;
    public float MaxWallAngle = 120;
}