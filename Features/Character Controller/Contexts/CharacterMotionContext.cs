using Remedy.Framework;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(CharacterRaycastContext))]
public class CharacterMotionContext : MonoBehaviour
{
    public ScriptableEventBoolean.Input JumpInput = new();
    public ScriptableEventVector2.Input MoveInput = new();

    public ScriptableEventBoolean.Output OnJump = new();
    public ScriptableEventVector3.Output OnMoveOnGround = new();
    public ScriptableEventBoolean.Output OnFall = new();

    [SchematicProperties]
    public CharacterMotionProperties Properties;
    public Transform CameraTransform { get; private set; }
    public Vector3 Velocity => _velocity;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public bool JumpPooled => _jumpPooled;

    [SerializeField]
    [HideInInspector]
    private bool _cached;
    private Transform _transform;
    private Rigidbody _rb;
    private CharacterRaycastContext _raycastContext;
    private SphereCollider _collider;

    private Vector3 _velocity;
    private Vector3 _horizontalVelocity;
    private Vector3[] _forces = new Vector3[10];
    private Vector3 _overrideVelocity = default;
    private int _forcesCount = 0;
    private float _dt = 0;
    private Vector3 _horizontalMovementDirection = default;
    private Vector3 _vec3Zero = new Vector3(0, 0, 0);
    private bool _jumpPooled = false;

    private bool _physicsStep = false;

    private void OnEnable()
    {
        CameraTransform = CameraSubsystem.Camera?.transform;

        if (CameraTransform == null && Camera.main != null)
        {
            CameraTransform = Camera.main.transform;
            Debug.LogWarning($"MotionContext: Global camera not found, defaulting to MainCamera ({CameraTransform.name})");
        }

        if (CameraTransform == null)
            Debug.LogError("MotionContext: No camera found for motions to reference!");

    }

    private void FixedUpdate()
    {
        _physicsStep = !_physicsStep;


        if(_physicsStep)
        {
            _raycastContext.PerformGroundCast();

            // apply forces
            for (int i = 0; i < _forcesCount; i++)
            {
                _velocity += _forces[i];
            }

            _horizontalVelocity.x = _velocity.x;
            _horizontalVelocity.z = _velocity.z;

            // rest
            bool isResting = false;

            if (_horizontalVelocity.sqrMagnitude + Mathf.Pow(Mathf.Abs(_velocity.y) * 10, 2) < Properties.MovementSnapThreshold) // *gringe for the magic numbers*
            {
                isResting = true;
                _rb.Sleep();
            }
            else
            {
                _horizontalMovementDirection = _horizontalVelocity.normalized;
            }

            // apply velocity
            if (!isResting)
            {
                _rb.WakeUp();

                HandleWallSlidingMovement();

                if (_overrideVelocity == _vec3Zero)
                    _rb.linearVelocity = _velocity;
                else
                {
                    _rb.linearVelocity = _overrideVelocity;

                    _overrideVelocity = _vec3Zero;
                }

                OnMoveOnGround?.Invoke(_horizontalVelocity);
            }
        }
        else
        {
            // reset velocity
            var rbVelocity = _rb.linearVelocity;

            _horizontalVelocity.x = rbVelocity.x;
            _horizontalVelocity.z = rbVelocity.z;
            _velocity = rbVelocity;

            // reset forces
            _forcesCount = 0;
        }

    }

    /// <summary>
    /// Applies the given force to the Rigidbody
    /// </summary>
    /// <param name="force"></param>
    /// <returns></returns>
    public void ApplyForce(Vector3 force, bool overrideOtherForces = false)
    {
        if (!overrideOtherForces)
        {
            if (_forcesCount < _forces.Length)
            {
                _forces[_forcesCount] = force;

                _forcesCount++;
            }
        }
        else
        {
            _overrideVelocity = force;
        }
    }

    public void ApplyHorizontalForce(Vector3 force, bool overrideOtherForces = false)
    {
        if (!overrideOtherForces)
        {
            if (_forcesCount < _forces.Length)
            {
                _forces[_forcesCount].x = force.x;
                _forces[_forcesCount].y = 0;
                _forces[_forcesCount].z = force.z;

                _forcesCount++;
            }
        }
        else
        {
            _overrideVelocity.x = force.x;
            _overrideVelocity.y = 0;
            _overrideVelocity.z = force.z;
        }
    }

    public void ApplyVerticalForce(float force, bool overrideOtherForces = false)
    {
        if (!overrideOtherForces)
        {
            if (_forcesCount < _forces.Length)
            {
                _forces[_forcesCount].x = 0;
                _forces[_forcesCount].y = force;
                _forces[_forcesCount].z = 0;

                _forcesCount++;
            }
        }
        else
        {
            _overrideVelocity.x = 0;
            _overrideVelocity.y = force;
            _overrideVelocity.z = 0;
        }
    }

    public Vector3 PredictNextPosition()
    {
        return _rb.position + _velocity * _dt
                  + 0.5f * Physics.gravity * (_dt * _dt);
    }

    public void ApplyGravity()
    {
        _velocity.y += Physics.gravity.y * _dt;
    }

    public void PoolJump()
    {
        _jumpPooled = true;
    }

    public void PerformJump()
    {
        _jumpPooled = false;
    }

    public Vector3 TransformInputDirection(Vector3 inputDirection)
    {
        return CameraTransform.TransformDirection(inputDirection);
    }

    private void EnsureReferenceCache()
    {
        if (!_cached)
        {
            _dt = Time.fixedDeltaTime;
            _transform = gameObject.GetCachedComponent<Transform>();
            _rb = gameObject.GetCachedComponent<Rigidbody>();
            _raycastContext = gameObject.GetCachedComponent<CharacterRaycastContext>();
            _collider = gameObject.GetCachedComponent<SphereCollider>();
            _collider.sharedMaterial = Properties.PhysicsMaterial;

            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.useGravity = false;

            _cached = true;
        }
    }

    private void HandleWallSlidingMovement()
    {
        _raycastContext.PerformWallCast(_horizontalMovementDirection);
        Vector3 _wallProjectedVelocity;

        if (_raycastContext.WallCastResult.TryGetClosestHit(out var closestWallHit))
        {
            var _wallNormal = closestWallHit.normal;
            _wallProjectedVelocity = Vector3.ProjectOnPlane(_horizontalVelocity, _wallNormal);
            ApplyHorizontalForce(_wallProjectedVelocity, true);
        }
    }

    private void OnDrawGizmos()
    {
        EnsureReferenceCache();

        Gizmos.color = new Color(0f, 0.3f, 1f, 0.75f); 
        Gizmos.DrawSphere(_transform.position + _collider.center, _collider.radius);

        for (int i = 0; i < _forcesCount; i++)
        {
            if (_forces[i] != Vector3.zero)
                CustomGizmos.DrawArrow(_transform.position, _forces[i], _forces[i].magnitude * 2, new Color(1f, 1f, 0f, 0.3f));
        }

        if (_rb.linearVelocity != Vector3.zero)
            CustomGizmos.DrawArrow(_transform.position, _rb.linearVelocity * 0.5f, _rb.linearVelocity.magnitude, new Color(0, 1f, 0, 0.5f));
    }
}