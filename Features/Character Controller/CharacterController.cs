using Remedy.Framework;
//using SaintsField;
using UnityEngine;

[SchematicComponent("Movement/3D/Default Character Controller")]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterRaycastContext))]
public class PhysicsBasedCharacterController : MonoBehaviour
{
    public ScriptableEventVector2.Input MoveInput => MotionContext.MoveInput;
    public ScriptableEventBoolean.Input JumpInput => MotionContext.JumpInput;
    public ScriptableEventVector3.Input MoveToPosition = new();
    
    //[SepTitle("Physics")]
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float Acceleration = 10f;
    public float Deceleration = 10f;

    [Header("Jump and Fall")]
    [Tooltip("The speed at which the character moves toward the desired Jump Height")]
    public float JumpSpeed = 10f;
    [Tooltip("The max height of the jump")]
    public float JumpHeight = 2f;
    public float FallSpeed = 2f;
    [Tooltip("The delay between transitioning from Grounded to Jumping states after the Character has landed")]
    public float JumpResetDelay = 1f;
    [Tooltip("The amount of time after the Jump Button has been initially pressed to consider whether a jump should occur")]
    public float JumpBuffer = 1f;
    [Tooltip("The amount of time to remain at the peak of a Jump (or push)")]
    public float HangTimeDuration = 1f;
    [Tooltip("Rate of change toward the desired Hang Time Height during Hang Time")]
    public float HangTimeDamper = 10f;

    [Header("Ride Height")]
    public float RideHeight = 1.5f;
    [Tooltip("Larger numbers can make the character jump innapropriately, but also make it stick to the desired height easier.")]
    public float MaxHoverForce = 0.5f;
    public float RideSpringStrength = 200f;
    public float VerticalSpringDamper = 20f;

    [Header("Collisions")]
    [Tooltip("The amount of time to fully recover from a Collision's Push")]
    public float CollisionRecoveryDuration = 1f;
    [Tooltip("The max amount of influence over moving velocity a Collision's push can have")]
    //[MinMaxSlider(0.0f, 1f)]
    public float CollisionInfluence = 0.25f;
    [Tooltip("Required speed of the moving object before the collision is considered a 'push'")]
    public float CollisionVelocityThreshold = 3f;

    [Header("Aesthetic")]
    public float TiltAmount = 10f;
    public float TiltReboundSpeed = 1f;

    public CharacterMotionContext MotionContext => _motionContext ??= gameObject.GetCachedComponent<CharacterMotionContext>();
    public CharacterRaycastContext RaycastContext => _raycastContext ??= gameObject.GetCachedComponent<CharacterRaycastContext>();

    // Cached
    private bool _cached = false;
    private CharacterRaycastContext _raycastContext;
    private CharacterMotionContext _motionContext;
    private Transform _transform;

    // Internal Values
    //private Vector3 _velocity = default;
    private Vector3 _wallProjectedVelocity = default;
    private float _targetYaw;
    private Vector3 _moveInput = default;
    private Vector3 _moveDirection = default;
    private Vector3 _wallNormal;

    private bool _prevGrounded = false;
    private bool _isJumping = false;
    private float _jumpResetTime = 0f;
    private float _jumpBufferTime = 0f;
    private float _jumpArcY = 0f;
    private bool _reachedJumpArc = false;

    private float _tempHangTimeDuration = -1; // > 0 = use this instead of default Hang Time
    private float _currentHangTime = 0f;
    private bool _physicsStep = false;
    private float _dt;
    private float _hoverHeight;
    private float _jumpStartGroundHeight;

    private void OnEnable()
    {
        if(!_cached)
        {
            _transform = gameObject.GetComponent<Transform>();
            _raycastContext = gameObject.GetCachedComponent<CharacterRaycastContext>();
            _motionContext = gameObject.GetCachedComponent<CharacterMotionContext>();
            _cached = true;
        }


        MoveInput?.Subscribe(this, (Vector2 value) =>
        {
            _moveInput.x = value.x;
            _moveInput.z = value.y;
        });

        JumpInput?.Subscribe(this, (bool value) =>
        {
            _moveInput.y = value ? 1 : 0;

            if (value)
                _jumpBufferTime = 0f;
        });
    }

    private void OnDisable()
    {
        MoveInput?.Unsubscribe(this);
        JumpInput?.Unsubscribe(this);
        MoveToPosition?.Unsubscribe(this);
    }

    private void FixedUpdate()
    {
        //Vector3 origin = _transform.position;

        _dt = Time.fixedDeltaTime;

        _physicsStep = !_physicsStep;

        if(_physicsStep)
        {
            HandleGroundRide();
            HandleHorizontalMovement();
            HandleJump();
        }
        else
        {
/*            _raycastContext.PerformWallCast(_moveDirection);

            if (_raycastContext.WallCastResult.TryGetClosestHit(out var closestWallHit))
            {
                _wallNormal = closestWallHit.normal;
                _wallProjectedVelocity = Vector3.ProjectOnPlane(_motionContext.HorizontalVelocity, _wallNormal);
            }

            _motionContext.ApplyHorizontalForce(_wallProjectedVelocity, true);*/
        }
    }

    private void HandleGroundRide()
    {
        if (_raycastContext.IsGrounded && !_isJumping)
        {
            if (_raycastContext.GroundCastResult.TryGetClosestHit(out var closestGroundHit))
            {
                SpringToY(closestGroundHit.point.y, RideHeight);
                _prevGrounded = true;
                _reachedJumpArc = false;
            }
        }
        else
        {
            _motionContext.ApplyGravity();
            _prevGrounded = false;
        }
    }

    private void HandleHorizontalMovement()
    {
        float desiredSpeed = MoveSpeed;
        float desiredRateOfChange = Acceleration;

        if (Mathf.Abs(_moveInput.x) + Mathf.Abs(_moveInput.z) < 0.1f)
        {
            desiredSpeed = 0;
            desiredRateOfChange = Deceleration;
        }

        _moveDirection = _moveInput;

        if (_motionContext.CameraTransform != null)
            _moveDirection = _motionContext.TransformInputDirection(_moveInput);

        _moveDirection.y = 0f;
        _moveDirection.Normalize();

        Vector3 desiredVel = (_moveDirection * desiredSpeed);

        Vector3 clampedVel = desiredVel.normalized * desiredSpeed;
        Vector3 excess = _motionContext.HorizontalVelocity - clampedVel;

        Vector3 correction = -excess.normalized * desiredRateOfChange;

        _motionContext.ApplyHorizontalForce(correction * _dt);

        if (Mathf.Abs(desiredVel.x) + Mathf.Abs(desiredVel.z) > 0.01f)
        {
            _targetYaw = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
        }
    }

    private void HandleJump()
    {
        float hangTimeDuration = _tempHangTimeDuration > 0 ? _tempHangTimeDuration : HangTimeDuration;

        if (_moveInput.y == 0)
        {
            if(_prevGrounded)
                _jumpResetTime += _dt;

            if(_isJumping)
            {
                _jumpBufferTime = JumpBuffer;
                _isJumping = false;
            }
        }

        if (!_isJumping && _prevGrounded)
        {
            _jumpResetTime += _dt;

            if (_jumpResetTime > JumpResetDelay && _jumpBufferTime < JumpBuffer)
            {
                if (_raycastContext.GroundCastResult.TryGetClosestHit(out var closestGroundHit))
                {
                    _isJumping = true;
                    _prevGrounded = false;
                    _jumpResetTime = 0f;
                    _jumpArcY = closestGroundHit.point.y + JumpHeight;

                    _motionContext.ApplyVerticalForce(JumpSpeed, true);

                    _jumpStartGroundHeight = closestGroundHit.point.y;
                    _reachedJumpArc = false;
                }
            }
        }
        else if(_isJumping)
        {
            Vector3 predictedPos = _motionContext.PredictNextPosition();

            if (predictedPos.y < _jumpArcY)
            {
                _motionContext.ApplyVerticalForce(JumpSpeed, true);
            }
            else
            {
                _isJumping = false;
                _currentHangTime = 0f;
                _reachedJumpArc = true;
            }
        }
        else if (_currentHangTime < hangTimeDuration)
        {
            if(_reachedJumpArc)
                SpringToY(_jumpStartGroundHeight, JumpHeight);
            else
            {
                if (_hoverHeight < _transform.position.y)
                    _hoverHeight = _transform.position.y;
                SpringToY(_jumpStartGroundHeight, _hoverHeight);
            }
            _currentHangTime += _dt;
        }
        else if(!_prevGrounded)
        {
            _motionContext.ApplyVerticalForce(-FallSpeed);
        }

        _jumpBufferTime += _dt;
    }


    private void SpringToY(float groundYPosition, float targetYPosition)
    {
        float distance = Mathf.Abs(Mathf.Abs(groundYPosition) - Mathf.Abs(_transform.position.y));
        float heightError = distance - targetYPosition;
        float velAlongNormal = Vector3.Dot(Vector3.down, _motionContext.Velocity);
        float differenceForce = (heightError * RideSpringStrength) - (velAlongNormal * VerticalSpringDamper);

        float finalForce = Mathf.Clamp((Physics.gravity.y + differenceForce) * _dt, -MaxHoverForce, MaxHoverForce);

        _motionContext.ApplyVerticalForce(-finalForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.down) > 0.5f)
            {
                _currentHangTime = HangTimeDuration; 
                break;
            }
        }
    }
}
