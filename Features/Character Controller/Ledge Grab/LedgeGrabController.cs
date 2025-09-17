using Remedy.Framework;
/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.CharacterControllers.LedgeGrab
{
    [RequireComponent(typeof(CharacterMotionContext))]
    [RequireComponent(typeof(CharacterRaycastContext))]
    [SchematicComponent("Movement/3D/Ledge Grab Controller")]
    //[Searchable]
    public class LedgeGrabController : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        public ScriptableEventBoolean.Input JumpInput => MotionContext.JumpInput;
        public ScriptableEventVector2.Input MoveInput => MotionContext.MoveInput;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        public ScriptableEvent.Output OnLedgeGrabStart;
        public ScriptableEvent.Output OnLedgeGrabEnd;


        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]
        [Dropdown("GetProperties")]*/
        public LedgeGrabControllerProperties Properties;

        [SchematicProperties]
        //[Expandable]
        public LedgeGrabControllerProperties PropertiesData;
        public CharacterMotionContext MotionContext => _motionContext ??= gameObject.GetCachedComponent<CharacterMotionContext>();
        public CharacterRaycastContext RaycastContext => _raycastContext ??= gameObject.GetCachedComponent<CharacterRaycastContext>();

        /*public DropdownList<LedgeGrabControllerProperties> GetProperties()
        {
            var list = new DropdownList<LedgeGrabControllerProperties>();

            if (LedgeGrabControllerProperties.Lookup.Keys.Count == 0)
                list.Add("None Created", null);
            else
            {
                foreach (var item in LedgeGrabControllerProperties.Lookup.Values)
                {
                    list.Add(item.name, item);
                }
            }

            return list;
        }

        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        public RaycastHit LastWallHit;
        public Quaternion FacingRotation;
        public Vector3 LedgePosition = Vector3.zero;
        public Vector2 MoveAxisInput = Vector2.zero;

        public bool IsClambering = false;

        private bool _cached = false;
        private CharacterMotionContext _motionContext;
        private CharacterRaycastContext _raycastContext;
        private Transform _transform;
        private Rigidbody _rb;

        private Vector3 _jumpDir = Vector3.zero;
        private Vector3 _moveInput;
        Vector3 directionToWall;

        public float Dir = 1;

        public enum WallSide { Left, Right }
        public WallSide CurrentWallSide { get; private set; }

        private int _jumpWaitTime = 0;
        private Vector3 _ogPosition = Vector3.zero;

        private int _translateTime = 0;
        private bool _translating = false;
        private bool _clamber = false;
        private RaycastHit _closestHit;
        private bool _alreadyPositioned = false;
        private Vector3 _wallNormal;
        private Vector3 _facingDir;
        private Vector3 _euler;
        private Quaternion _rotation;

        private void OnValidate()
        {
            PropertiesData = Properties;
        }

        private void OnEnable()
        {
            if(_cached)
            {
                _transform = transform;
                _rb = gameObject.GetCachedComponent<Rigidbody>();
                _motionContext = gameObject.GetCachedComponent<CharacterMotionContext>();
                _raycastContext = gameObject.GetCachedComponent<CharacterRaycastContext>();
                _cached = true;
            }

            JumpInput?.Subscribe(this, (bool value) =>
            {
                if (!value) return;
                _clamber = true;
                _translating = true;
                IsClambering = true;
            });
            MoveInput?.Subscribe(this, Move);

            OnLedgeGrabStart?.Invoke(true);

            _rb.freezeRotation = true;

            _jumpWaitTime = 0;
            _rb.isKinematic = true;

            MoveAxisInput = Vector2.zero;

            _ogPosition = _transform.position;
            _translating = false;
            _translateTime = 0;
        }

        private void OnDisable()
        {
            OnLedgeGrabEnd?.Invoke(false);
            _rb.isKinematic = false;
            _alreadyPositioned = false;
            LedgePosition = Vector3.zero;

            JumpInput?.Unsubscribe(this);
            MoveInput?.Unsubscribe(this);
        }

        private void Update()
        {
            if (!_alreadyPositioned) return;
            if (LedgePosition == Vector3.zero)
            {
                enabled = false;
                return;
            }

            _moveInput = new Vector3(MoveAxisInput.x, 0, MoveAxisInput.y);
            _moveInput = _motionContext.TransformInputDirection(_moveInput);

            _rb.linearVelocity = Vector3.zero;

            _jumpWaitTime++;

            if (_moveInput != Vector3.zero && _jumpWaitTime > Properties.OrientationTime)
            {
                if (Vector3.Dot(directionToWall.normalized, _moveInput) > 0f)
                {
                    _clamber = true;
                    _translating = true;
                    IsClambering = true;
                }
                else
                {
                    _clamber = false;
                    _translating = true;
                }
            }

            if (_translating)
            {
                if (_clamber)
                {
                    _rb.transform.position = LedgePosition + new Vector3(0, Properties.LedgeOffset, 0);
                    _transform.position = LedgePosition + new Vector3(0, Properties.LedgeOffset, 0);

                    if (_translateTime >= Properties.ClamberTime)
                    {
                        enabled = false;
                    }
                    _translateTime++;
                }
                else
                {
                    _rb.transform.position = _ogPosition;
                    _transform.position = _ogPosition;
                    enabled = false;
                }
            }

            _raycastContext.PerformWallCast(_moveInput);

            if (_raycastContext.WallCastResult.TryGetClosestHit(out RaycastHit wallHit))
                StickOnWall(wallHit);
        }

        public void Move(Vector2 input)
        {
            if (_jumpWaitTime > Properties.OrientationTime)
                MoveAxisInput = input;
        }


        private void StickOnWall(RaycastHit hit)
        {
            if (_alreadyPositioned) return;

            if (Vector3.Distance(_transform.position, hit.point) > 3) enabled = false;

            LastWallHit = hit;

            // Direction to the wall
            directionToWall = hit.point - _transform.position;
            directionToWall.Normalize();

            // Get the wall normal
            _wallNormal = hit.normal;

            _facingDir = Vector3.ProjectOnPlane(directionToWall.normalized, _wallNormal).normalized;
            _facingDir.y = 0;

            // Face along the wall with "up" preserved
            FacingRotation = Quaternion.LookRotation(_facingDir, Vector3.up);

            _euler = FacingRotation.eulerAngles;
            _euler.y -= 90;

            _rotation = Quaternion.Euler(_euler);
            _jumpDir = _rotation * Vector3.forward;
            _jumpDir.y = 0;

            FacingRotation = Quaternion.LookRotation(_jumpDir, Vector3.up);
            CurrentWallSide = Vector3.Dot(directionToWall, _jumpDir) > 0 ? WallSide.Left : WallSide.Right;

            _closestHit = hit;
            AdjustHandPosition(hit.point);
            _alreadyPositioned = true;
        }

        private void AdjustHandPosition(Vector3 wallContactPoint)
        {
            if (CurrentWallSide == WallSide.Right)
            {
                if (RaycastUtility.SphereCast(_transform.position + new Vector3(0, Properties.LedgeOffset, 0) - _jumpDir, Vector3.down, Properties.LedgeCheckRadius, Properties.LedgeOffset, _raycastContext.Properties.CollisionMask).TryGetClosestHit(out RaycastHit ledgeHit))
                {
                    LedgePosition = new Vector3(wallContactPoint.x, ledgeHit.point.y, wallContactPoint.z);
                }
            }
            else
            {
                if (RaycastUtility.SphereCast(_transform.position + new Vector3(0, Properties.LedgeOffset, 0) + _jumpDir, Vector3.down, Properties.LedgeCheckRadius, Properties.LedgeOffset, _raycastContext.Properties.CollisionMask).TryGetClosestHit(out RaycastHit ledgeHit))
                {
                    LedgePosition = new Vector3(wallContactPoint.x, ledgeHit.point.y, wallContactPoint.z);
                }
            }
            if (Vector3.Distance(_transform.position, LedgePosition) > 3) enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && _rb != null && Properties != null)
            {
                Gizmos.color = Color.gray;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_transform.position, _jumpDir * 2f); // shows jump direction

            Gizmos.DrawCube(_closestHit.point, Vector3.one);
        }
    }
}