using Remedy.Framework;
/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.CharacterControllers.Hover
{
    [RequireComponent(typeof(CharacterMotionContext))]
    [RequireComponent(typeof(CharacterRaycastContext))]
    [SchematicComponent("Movement/3D/Flight and Hover Controller")]
    //[Searchable]
    public class HoverController : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        public ScriptableEventBoolean.Input JumpInput => MotionContext.JumpInput;
        public ScriptableEventVector2.Input MoveInput => MotionContext.MoveInput;
        public ScriptableEventFloat.Input StaminaUpdated = new();

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]
        [Dropdown("GetProperties")]*/
        public HoverControllerProperties Properties;
        [SchematicProperties]
        /*[Expandable]
        [ReadOnly]*/
        public HoverControllerProperties PropertiesData;
/*
        public DropdownList<HoverControllerProperties> GetProperties()
        {
            var list = new DropdownList<HoverControllerProperties>();

            if (HoverControllerProperties.Lookup.Keys.Count == 0)
                list.Add("None Created", null);
            else
            {
                foreach (var item in HoverControllerProperties.Lookup.Values)
                {
                    list.Add(item.name, item);
                }
            }

            return list;
        }

        
        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        public bool IsJumping = false;
        public Vector2 MoveAxisInput = Vector2.zero;
        public float CurrentVerticalSpeed = 0f;
        public float RotationVelocity;
        public CharacterMotionContext MotionContext => _motionContext ??= gameObject.GetCachedComponent<CharacterMotionContext>();
        public CharacterRaycastContext RaycastContext => _raycastContext ??= gameObject.GetCachedComponent<CharacterRaycastContext>();

        private float _currentStamina;

        private Vector3 _moveInput;
        private Vector3 _m_GoalVel = Vector3.zero;
        private Vector3 _ogVelocity = Vector3.zero;
        private bool _hasRecievedMoveInput = false;
        private float _currentHoverTime = 0f;
        private float _currentFallSpeed = 0f;

        private bool _cached = false;
        private CharacterMotionContext _motionContext;
        private CharacterRaycastContext _raycastContext;
        private Rigidbody _rb;
        private Transform _transform;

        private void OnValidate()
        {
            PropertiesData = Properties;
        }

        private void OnEnable()
        {
            if(!_cached)
            {
                _motionContext = gameObject.GetCachedComponent<CharacterMotionContext>();
                _raycastContext = gameObject.GetCachedComponent<CharacterRaycastContext>();
                _rb = gameObject.GetCachedComponent<Rigidbody>();
                _transform = transform;
                _cached = true;
            }

            MoveInput?.Subscribe(this, Move);
            JumpInput?.Subscribe(this, Jump);

            _ogVelocity = _rb.linearVelocity;
            _ogVelocity.y = 0;
            _rb.linearVelocity = _ogVelocity;
            _rb.freezeRotation = true;

            StaminaUpdated?.Subscribe(this, (float value) => _currentStamina = value);

            IsJumping = true;
            _hasRecievedMoveInput = false;

            _moveInput.x = _rb.linearVelocity.normalized.x;
            _moveInput.z = _rb.linearVelocity.normalized.z;

            _currentHoverTime = 0f;
            _currentFallSpeed = 0f;
        }

        private void OnDisable()
        {
            MoveInput?.Unsubscribe(this);
            JumpInput?.Unsubscribe(this);
            StaminaUpdated?.Unsubscribe(this);
        }

        private void FixedUpdate()
        {
            if (_hasRecievedMoveInput)
            {
                _moveInput = new Vector3(MoveAxisInput.x, 0, MoveAxisInput.y);
                _moveInput = _motionContext.TransformInputDirection(_moveInput);
            }

            // Invoking Stamina Use with a value of 0 forces current stamina to update first
            if(_currentStamina > Properties.staminaUseRate * Time.fixedDeltaTime)
            {
                _rb.AddForce(Vector3.up * Properties.RiseSpeed);
            }
            else
            {
                if(_currentHoverTime < Properties.HoverTime)
                {
                    _rb.AddForce(Vector3.down * Properties.HoverFallSpeed);
                    _currentHoverTime += Time.fixedDeltaTime;
                    _currentFallSpeed = 0f;
                }
                else
                {
                    if(_currentFallSpeed < Properties.PlummetTerminalSpeed)
                        _currentFallSpeed += Properties.PlummetAcceleration;
                    _rb.AddForce(Vector3.down * _currentFallSpeed);
                }
            }
            CharacterMove(_moveInput);

            Quaternion targetRotation = Quaternion.LookRotation(_moveInput.normalized, Vector3.up);

            Quaternion rotationDifference = targetRotation * Quaternion.Inverse(_rb.rotation);

            rotationDifference.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 180f)
            {
                angle -= 360f;
            }

            Vector3 torque = axis * angle * Mathf.Deg2Rad * 10f;
            _rb.AddTorque(torque, ForceMode.Acceleration);

            _moveInput = Vector3.Lerp(_moveInput, Vector3.zero, Properties.InputNormalizationSpeed * Time.fixedDeltaTime);
        }

        public void Jump(bool value)
        {
            IsJumping = value;
        }

        public void Move(Vector2 value)
        {
            MoveAxisInput = value;
            _hasRecievedMoveInput = true;
        }

        private float _acceleration = 35f;
        private float _maxAccelForce = 55f;
        private float _speedFactor = 50f;
        private Vector3 _moveForceScale = Vector3.one;
        private float _leanFactor = 5f;


        private void CharacterMove(Vector3 moveInput)
        {
            Vector3 m_UnitGoal = moveInput;
            Vector3 unitVel = _m_GoalVel.normalized;
            float velDot = Vector3.Dot(m_UnitGoal, unitVel);
            float accel = _acceleration;
            Vector3 goalVel = m_UnitGoal * Properties.HorizontalSpeed * _speedFactor;
            Vector3 otherVel = Vector3.zero;
            _m_GoalVel = Vector3.MoveTowards(_m_GoalVel,
                                            goalVel,
                                            accel * Time.fixedDeltaTime);
            Vector3 neededAccel = (_m_GoalVel - _rb.linearVelocity) / Time.fixedDeltaTime;
            float maxAccel = _maxAccelForce;
            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
            _rb.AddForceAtPosition(Vector3.Scale(neededAccel * _rb.mass, _moveForceScale), _transform.position + new Vector3(0f, _transform.localScale.y * _leanFactor, 0f)); 
        }
    }
}