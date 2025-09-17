using Remedy.Framework;
using UnityEngine;

namespace Remedy.CharacterControllers.WallSlide
{
    [RequireComponent(typeof(CharacterMotionContext))]
    [RequireComponent(typeof(CharacterRaycastContext))]
    [SchematicComponent("Movement/3D/Wall Slide Controller")]
    public class WallSlideController : MonoBehaviour
    {
        public Rigidbody Rigidbody => gameObject.GetCachedComponent<Rigidbody>();

        public ScriptableEventBoolean.Input JumpInput => MotionContext.JumpInput;
        public ScriptableEventVector2.Input MoveInput => MotionContext.MoveInput;

        public ScriptableEvent.Output OnWallJump = new();
        public ScriptableEvent.Output OnWallSlideExitted = new();

        public WallSlideControllerProperties Properties;

        [SchematicProperties]
        public WallSlideControllerProperties PropertiesData;
        public CharacterMotionContext MotionContext => _motionContext ??= gameObject.GetCachedComponent<CharacterMotionContext>();
        public CharacterRaycastContext RaycastContext => _raycastContext ??= gameObject.GetCachedComponent<CharacterRaycastContext>();

        public RaycastHit LastWallHit;
        public Quaternion FacingRotation;

        private Rigidbody _rb => gameObject.GetCachedComponent<Rigidbody>();
        private Vector3 _gravitationalForce;
        private bool _alreadyEnded = false;
        private PhysicsBasedCharacterController _characterController => gameObject.GetCachedComponent<PhysicsBasedCharacterController>();

        private bool _jumpInProgress = false;
        private Vector3 _jumpDir = Vector3.zero;

        public float Dir = 1;

        public enum WallSide { Left, Right }
        public WallSide CurrentWallSide { get; private set; }

        private int _jumpWaitTime = 0;
        private Vector3 _directionToWall;
        private Vector3 _wallNormal;
        private Vector3 _facingDir;
        private Vector3 _euler;
        private Quaternion _rotation;
        private bool _wasStuckOnWall = false;
        private bool _isGrounded = false;

        private CharacterMotionContext _motionContext;
        private CharacterRaycastContext _raycastContext;

        private void OnValidate()
        {
            PropertiesData = Properties;
        }

        private void OnEnable()
        {
            _gravitationalForce.y = -Properties.WallSlideSpeed;

            _rb.freezeRotation = true;
            _alreadyEnded = false;

            _jumpWaitTime = 0;
            _wasStuckOnWall = false;
        }

        private void OnDisable()
        {
            _jumpInProgress = false;

            JumpInput?.Unsubscribe(this);
        }

        private void FixedUpdate()
        {
            if(_rb.isKinematic)
                _rb.isKinematic = false;

            if (!_jumpInProgress)
            {
                _rb.AddForce(_gravitationalForce);
                _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, Properties.WallFriction * Time.fixedDeltaTime);
            }

            if (_isGrounded && !_alreadyEnded && !_jumpInProgress)
            {
                _alreadyEnded = true;
                OnWallSlideExitted?.Invoke(false);
            }

            _jumpWaitTime += 1;


            float closestHitDistance = 99;
            for (int i = 0; i < 8; i++)
            {
                _raycastContext.PerformWallCast(new Vector3(Mathf.Sin(i * 45), 0, Mathf.Cos(i * 45)));

                if (_raycastContext.WallCastResult.TryGetClosestHit(out RaycastHit wallHit))
                {
                    if (wallHit.distance < closestHitDistance)
                    {
                        closestHitDistance = wallHit.distance;
                        _wasStuckOnWall = false;
                        StickOnWall(wallHit);
                    }
                }
            }

        }

        public void Jump(bool value)
        {
            if (_jumpInProgress || !enabled || _jumpWaitTime < Properties.OrientationTime || !value)
                return;

            _rb.isKinematic = true;
            _rb.rotation = FacingRotation;
            _rb.isKinematic = false;

            _jumpInProgress = true;

            OnWallJump?.Invoke(default);

            _rb.AddForce(Vector3.up * Properties.JumpForce, ForceMode.Impulse);

            if (CurrentWallSide == WallSide.Right)
                _rb.AddForce(_jumpDir * Properties.JumpOffForce, ForceMode.Impulse);

            if (CurrentWallSide == WallSide.Left)
                _rb.AddForce(-_jumpDir * Properties.JumpOffForce, ForceMode.Impulse);

            _rb.rotation = Quaternion.LookRotation(_jumpDir, Vector3.up);

            enabled = false;
        }


        private void StickOnWall(RaycastHit hit)
        {
            if (_wasStuckOnWall) return;
            LastWallHit = hit;

            // Direction to the wall
            _directionToWall = hit.point - transform.position;

            // Project character's forward vector onto the wall plane
            _facingDir = Vector3.ProjectOnPlane(_directionToWall.normalized, hit.normal).normalized;
            _facingDir.y = 0;

            // Prevent zero vectors causing LookRotation errors
            if (_facingDir == Vector3.zero)
                _facingDir = Vector3.ProjectOnPlane(-transform.right, hit.normal).normalized;

            // Face along the wall with "up" preserved
            FacingRotation = Quaternion.LookRotation(_facingDir, Vector3.up);

            _euler = FacingRotation.eulerAngles;
            _euler.y -= 90;

            _rotation = Quaternion.Euler(_euler);
            _jumpDir = _rotation * Vector3.forward; // Or: rotation.forward
            _jumpDir.y = 0;

            CurrentWallSide = Vector3.Dot(_directionToWall, _jumpDir) > 0 ? WallSide.Left : WallSide.Right;

            _wasStuckOnWall = true;
        }

        // Optional: Visualize the wall check ray in the Scene view
/*        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && Rigidbody != null && Properties != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawRay(transform.position, Rigidbody.linearVelocity * Properties.WallCheckDistance);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _jumpDir * 2f); // shows jump direction
        }*/
    }
}