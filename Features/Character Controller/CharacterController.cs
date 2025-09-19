using Remedy.Framework;
//using SaintsField;
using UnityEngine;

[SchematicComponent("Movement/3D/Default Character Controller")]
[RequireComponent(typeof(CharacterMotionContext))]
[RequireComponent(typeof(CharacterRaycastContext))]
public class PhysicsBasedCharacterController : MonoBehaviour
{
    [EventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.MoveInput))]
    public ScriptableEventVector2.Input MoveInput => MotionContext.MoveInput;
    [EventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.JumpInput))]
    public ScriptableEventBoolean.Input JumpInput => MotionContext.JumpInput;
    public ScriptableEventVector3.Input MoveToPosition = new();

    [SchematicProperties]
    public CharacterControllerProperties Properties;

    public CharacterMotionContext MotionContext => _motionContext ??= gameObject.GetComponent<CharacterMotionContext>();
    public CharacterRaycastContext RaycastContext => _raycastContext ??= gameObject.GetComponent<CharacterRaycastContext>();

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
    private float _dt;
    private float _hoverHeight;
    private float _jumpStartGroundHeight;
    private float _groundY = 0;
    private bool _hadJumped = false;

    private void OnEnable()
    {
        if(!_cached)
        {
            _transform = gameObject.GetComponent<Transform>();
            _raycastContext = gameObject.GetCachedComponent<CharacterRaycastContext>();
            _motionContext = gameObject.GetCachedComponent<CharacterMotionContext>();
            _cached = true;
        }

        _currentHangTime = Properties.HangTimeDuration;
        _jumpBufferTime = Properties.JumpBuffer;

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
        _dt = Time.fixedDeltaTime;
        HandleGroundRide();
        HandleHorizontalMovement();
        HandleJump();
    }

    private void HandleGroundRide()
    {
        if (_raycastContext.IsGrounded && !_isJumping)
        {
            _hadJumped = false;
            _groundY = _raycastContext.GroundPosition.y;
            _prevGrounded = true;
            _reachedJumpArc = false;
            SpringToY(_groundY, Properties.RideHeight);
        }
        else
        {
            _motionContext.ApplyGravity();
            _prevGrounded = false;
        }
    }

    private void HandleHorizontalMovement()
    {
        float desiredSpeed = Properties.MoveSpeed;
        float desiredRateOfChange = Properties.Acceleration;

        if (Mathf.Abs(_moveInput.x) + Mathf.Abs(_moveInput.z) < 0.1f)
        {
            desiredSpeed = 0;
            desiredRateOfChange = Properties.Deceleration;
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
        float hangTimeDuration = _tempHangTimeDuration > 0 ? _tempHangTimeDuration : Properties.HangTimeDuration;

        if (_moveInput.y == 0)
        {
            if(_prevGrounded)
                _jumpResetTime += _dt;

            if(_isJumping)
            {
                _jumpBufferTime = Properties.JumpBuffer;
                _isJumping = false;
            }
        }

        if (!_isJumping && _prevGrounded)
        {
            _jumpResetTime += _dt;

            if (_jumpResetTime > Properties.JumpResetDelay && _jumpBufferTime < Properties.JumpBuffer)
            {
                _isJumping = true;
                _prevGrounded = false;
                _jumpResetTime = 0f;
                _jumpArcY = _raycastContext.GroundPosition.y + Properties.JumpHeight;

                _motionContext.ApplyVerticalForce(Properties.JumpSpeed, true);

                _jumpStartGroundHeight = _raycastContext.GroundPosition.y;
                _reachedJumpArc = false;

                _hadJumped = true;
            }
        }
        else if(_isJumping)
        {
            Vector3 predictedPos = _motionContext.PredictNextPosition();

            if (predictedPos.y < _jumpArcY)
            {
                _motionContext.ApplyVerticalForce(Properties.JumpSpeed, true);
            }
            else
            {
                _isJumping = false;
                _currentHangTime = 0f;
                _reachedJumpArc = true;
            }
        }
        else if (_currentHangTime < hangTimeDuration && _hadJumped)
        {
            if(_reachedJumpArc)
                SpringToY(_jumpStartGroundHeight, Properties.JumpHeight);
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
            _motionContext.ApplyVerticalForce(-Properties.FallSpeed);
        }

        _jumpBufferTime += _dt;
    }


    private void SpringToY(float groundYPosition, float targetYPosition)
    {
        float distance = Mathf.Abs(Mathf.Abs(groundYPosition) - Mathf.Abs(_transform.position.y));
        float heightError = distance - targetYPosition;
        float velAlongNormal = Vector3.Dot(Vector3.down, _motionContext.Velocity);
        float differenceForce = (heightError * Properties.RideSpringStrength) - (velAlongNormal * Properties.VerticalSpringDamper);

        float finalForce = Mathf.Clamp((differenceForce) * _dt, -Properties.MaxHoverForce, Properties.MaxHoverForce);

        _motionContext.ApplyVerticalForce(-finalForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.down) > 0.5f)
            {
                _currentHangTime = Properties.HangTimeDuration; 
                break;
            }
        }
    }
}
