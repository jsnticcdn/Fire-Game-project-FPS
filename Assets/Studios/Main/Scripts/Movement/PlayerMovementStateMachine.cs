using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AZE.AdvancedFirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerMovementStateMachine : MonoBehaviour
    {
        private const float MoveInputDeadzone = 0.1f;
        private const float MoveInputDeadzoneSqr = MoveInputDeadzone * MoveInputDeadzone;
        private const int MaxTransitionsPerFrame = 4;

        [Header("Speed Settings")]
        [Tooltip("Grounded movement speed, in m/s.")]
        [Range(1f, 10f)] public float WalkSpeed = 4.5f;
        [Tooltip("Sprint speed, in m/s.")]
        [Range(5f, 20f)] public float RunSpeed = 7f;
        [Tooltip("Movement speed while crouched, in m/s.")]
        [Range(1f, 5f)] public float CrouchSpeed = 2.5f;
        [Tooltip("Interpolation rate for acceleration and deceleration. Framerate-independent.")]
        [Range(0f, 30f)] public float MovementSmoothing = 15f;
        [Tooltip("When enabled, sprinting requires forward input.")]
        public bool SprintForwardOnly = false;

        [Header("Physics Settings")]
        [Tooltip("Downward acceleration, in m/s².")]
        public float Gravity = -15f;
        [Tooltip("Downward velocity applied while grounded. More negative values stick harder to abrupt geometry changes.")]
        public float InitialFallVelocity = -3f;

        [Header("Ground Detection")]
        [Tooltip("Layers that count as walkable ground. The player's own collider is always ignored.")]
        public LayerMask GroundLayers = ~0;
        [Tooltip("How far below the capsule the ground probe reaches before the player is considered airborne.")]
        [Range(0.01f, 0.5f)] public float GroundCheckDistance = 0.15f;
        [Tooltip("How far the capsule may be pulled back down to keep contact when running downhill. 0 disables snapping.")]
        [Range(0f, 1f)] public float GroundSnapDistance = 0.3f;

        [Header("Slope Settings")]
        [Tooltip("Slide down surfaces steeper than the CharacterController's Slope Limit.")]
        [FormerlySerializedAs("useSlopeSlide")] public bool UseSlopeSlide = true;
        [Tooltip("Downhill speed on a vertical surface. Shallower slopes slide proportionally slower.")]
        [Range(1f, 20f)] public float SlopeSlideSpeed = 8f;
        [Tooltip("Steering kept while slope sliding, as a fraction of Walk Speed.")]
        [Range(0f, 1f)] public float SlopeSlideControl = 0.25f;

        [Header("Jump Settings")]
        [Tooltip("Enable jumping.")]
        [FormerlySerializedAs("useJump")] public bool UseJump = true;
        [Tooltip("Upward velocity applied on jump.")]
        [Range(1f, 10f)] public float JumpForce = 5f;
        [Tooltip("Seconds the player can still jump after walking off a ledge.")]
        [Range(0.01f, 0.5f)] public float CoyoteTime = 0.2f;
        [Tooltip("Seconds a jump input is stored before hitting the ground.")]
        [Range(0.01f, 0.5f)] public float JumpBufferTime = 0.2f;

        [Header("Air Control")]
        [Tooltip("How much input redirects momentum while airborne. 1 gives full steering, 0 locks the launch trajectory.")]
        [Range(0f, 1f)] public float AirControl = 1f;
        [Tooltip("How much horizontal speed each consecutive jump keeps.")]
        [Range(0f, 1f)] public float JumpSpeedRetention = 0.9f;
        [Tooltip("Interpolation rate for airborne direction changes.")]
        [Range(0f, 30f)] public float AirSmoothing = 6f;

        [Header("Crouch Settings")]
        [Tooltip("Enable crouching.")]
        [FormerlySerializedAs("useCrouch")] public bool UseCrouch = true;
        [Tooltip("Interpolation rate of the capsule height change.")]
        [Range(0f, 20f)] public float CrouchTransitionSpeed = 10f;
        [Tooltip("Crouched capsule height as a fraction of standing height.")]
        [Range(0.3f, 0.9f)] public float CrouchHeightRatio = 0.5f;
        [Tooltip("Distance the camera sits below the top of the capsule.")]
        public float CameraOffset = 0.15f;
        [Tooltip("Layers that can block the player from standing up.")]
        public LayerMask CeilingLayers = ~0;

        [Header("Slide Settings")]
        [Tooltip("Enable the crouch slide.")]
        public bool UseSlide = true;
        [Tooltip("Minimum horizontal speed for a crouch press to become a slide. Set it between Walk Speed and Run Speed.")]
        [Range(0f, 20f)] public float SlideMinSpeed = 6f;
        [Tooltip("Slide deceleration on flat ground, in m/s².")]
        [Range(0f, 20f)] public float SlideFriction = 4.5f;
        [Tooltip("How strongly slopes accelerate or brake the slide, scaled by steepness and alignment with the fall line.")]
        [Range(0f, 30f)] public float SlideSlopeAcceleration = 15f;
        [Tooltip("Speed cap on long downhill slides.")]
        [Range(5f, 30f)] public float SlideMaxSpeed = 15f;
        [Tooltip("How fast the slide direction can be steered, in radians per second. 0 locks the launch direction.")]
        [Range(0f, 5f)] public float SlideSteering = 1.5f;

        [Header("Dodge Settings")]
        [Tooltip("Enable the dodge dash.")]
        [FormerlySerializedAs("useDodge")] public bool UseDodge = true;
        [Tooltip("Initial dash velocity, in m/s.")]
        [Range(10f, 30f)] public float DodgeSpeed = 15f;
        [Tooltip("Dash duration, in seconds.")]
        [Range(0f, 1f)] public float DodgeDuration = 0.3f;
        [Tooltip("Seconds required between dashes.")]
        [Range(0f, 5f)] public float DodgeCooldown = 3f;

        [Header("References")]
        [Tooltip("The camera pivot moved by the crouch and read by the dodge. Must be a child of the player.")]
        public Transform CameraTransform;

        public event Action<PlayerBaseState, PlayerBaseState> StateChanged;
        public event Action<float> Landed;

        public PlayerInputHandler InputHandler { get; private set; }
        public CharacterController Controller { get; private set; }

        public Vector3 CurrentMoveVelocity { get; set; }
        public float VerticalVelocity { get; set; }

        public bool IsGrounded => _ground.IsGrounded;
        public Vector3 GroundNormal => _ground.Normal;
        public float GroundAngle => _ground.Angle;
        public bool OnSteepGround => _ground.OnSteepGround;

        public float CoyoteTimeCounter { get; set; }
        public float JumpBufferCounter { get; set; }
        public float LastDodgeTime { get; set; } = -10f;

        public float AirSpeed { get; private set; }

        public float StandingHeight => _height.StandingHeight;
        public float CrouchHeight => _height.CrouchHeight;

        public float TargetHeight
        {
            get => _height.TargetHeight;
            set => _height.TargetHeight = value;
        }

        public PlayerBaseState CurrentState => _currentState;
        public PlayerStateFactory States => _states;

        private PlayerBaseState _currentState;
        private PlayerStateFactory _states;
        private GroundSensor _ground;
        private CapsuleHeightController _height;
        private float _defaultStepOffset;
        private bool _wasGrounded;

        public bool HasMoveInput => InputHandler.MoveInput.sqrMagnitude > MoveInputDeadzoneSqr;

        public bool SprintHeld => InputHandler.SprintPressed && (!SprintForwardOnly || InputHandler.MoveInput.y > MoveInputDeadzone);

        public bool JumpRequested => UseJump && !OnSteepGround && JumpBufferCounter > 0f && CoyoteTimeCounter > 0f;

        public bool CrouchRequested => UseCrouch && InputHandler.CrouchTriggered;

        public bool ShouldSlopeSlide => UseSlopeSlide && OnSteepGround;

        public bool SlideRequested
        {
            get
            {
                if (!UseSlide || !CrouchRequested) return false;
                if (!IsGrounded || OnSteepGround) return false;

                Vector3 horizontal = CurrentMoveVelocity;
                horizontal.y = 0f;

                return horizontal.magnitude >= SlideMinSpeed;
            }
        }

        public float CurrentSpeedPercentage
        {
            get
            {
                if (Controller == null) return 0f;

                Vector3 velocity = Controller.velocity;
                velocity.y = 0f;

                return Mathf.Clamp01(velocity.magnitude / RunSpeed);
            }
        }

        private void Awake()
        {
            InputHandler = GetComponent<PlayerInputHandler>();
            Controller = GetComponent<CharacterController>();

            if (CameraTransform == null)
            {
                Debug.LogError("[AZE] PlayerMovementStateMachine: CameraTransform is not assigned.", this);
                enabled = false;
                return;
            }

            if (!InputHandler.HasActionsAssigned)
            {
                Debug.LogError("[AZE] PlayerMovementStateMachine: PlayerInputHandler is missing Input Action References.", this);
                enabled = false;
                return;
            }

            _states = new PlayerStateFactory(this);
            _ground = new GroundSensor(this);
            _height = new CapsuleHeightController(this);

            _defaultStepOffset = Controller.stepOffset;
            Controller.minMoveDistance = 0f;

            AirSpeed = WalkSpeed;

            UpdateGroundState();
            _wasGrounded = IsGrounded;

            _currentState = _states.Idle;
            _currentState.Enter();
        }

        private void Update()
        {
            Vector3 groundDelta = _ground.Ride();
            Physics.SyncTransforms();

            UpdateGroundState();
            HandleGroundContactChanges();
            TrackGroundedSpeed();
            UpdateTimers();

            ResolveTransitions();

            HandleGravity();
            ApplyIntent(_currentState.BuildIntent());

            ApplyFinalMovement();
            HandleCeilingCollision();
            SnapToGround(groundDelta);
            _height.Interpolate();
        }

        public void SwitchState(PlayerBaseState newState)
        {
            PlayerBaseState previousState = _currentState;

            previousState.Exit();
            _currentState = newState;
            _currentState.Enter();

            StateChanged?.Invoke(previousState, newState);
        }

        public Vector3 GetInputWorldVector()
        {
            Vector2 input = InputHandler.MoveInput;

            return transform.right * input.x + transform.forward * input.y;
        }

        public void ApplyIntent(MovementIntent intent)
        {
            CurrentMoveVelocity = intent.IsImmediate
                ? intent.TargetVelocity
                : Vector3.Lerp(CurrentMoveVelocity, intent.TargetVelocity, MotionMath.DampFactor(intent.Smoothing, Time.deltaTime));
        }

        public Vector3 BuildGroundedVelocity(float speed)
        {
            if (!HasMoveInput) return Vector3.zero;

            Vector3 moveDir = GetInputWorldVector();

            if (IsGrounded && !OnSteepGround)
            {
                moveDir = Vector3.ProjectOnPlane(moveDir, GroundNormal);
            }

            return moveDir.normalized * speed;
        }

        public Vector3 BuildAirborneVelocity(float speed)
        {
            Vector3 preserved = CurrentMoveVelocity;
            preserved.y = 0f;

            if (!HasMoveInput) return preserved;

            Vector3 steered = GetInputWorldVector().normalized * speed;

            return Vector3.Lerp(preserved, steered, AirControl);
        }

        public void ConstrainVelocityToGround()
        {
            CurrentMoveVelocity = Vector3.ProjectOnPlane(CurrentMoveVelocity, GroundNormal);
        }

        public void ApplyJumpSpeedPenalty()
        {
            AirSpeed = Mathf.Max(WalkSpeed, AirSpeed * JumpSpeedRetention);
        }

        public bool TryConsumeDodge()
        {
            if (!UseDodge || !InputHandler.DodgeTriggered) return false;
            if (!IsGrounded || OnSteepGround || InputHandler.CrouchTriggered) return false;
            if (Time.time < LastDodgeTime + DodgeCooldown) return false;

            InputHandler.ConsumeDodgeBuffer();
            return true;
        }

        public bool CanStandUp()
        {
            return _height.CanStandUp();
        }

        private void HandleGravity()
        {
            if (IsGrounded && VerticalVelocity < 0f)
            {
                VerticalVelocity = InitialFallVelocity;
            }
            VerticalVelocity += Gravity * Time.deltaTime;
        }

        private void TrackGroundedSpeed()
        {
            if (!IsGrounded) return;

            Vector3 horizontal = CurrentMoveVelocity;
            horizontal.y = 0f;

            AirSpeed = Mathf.Max(WalkSpeed, horizontal.magnitude);
        }

        private void UpdateTimers()
        {
            bool onWalkableGround = IsGrounded && !OnSteepGround;

            Controller.stepOffset = onWalkableGround ? _defaultStepOffset : 0f;

            JumpBufferCounter = InputHandler.JumpTriggered ? JumpBufferTime : JumpBufferCounter - Time.deltaTime;
            CoyoteTimeCounter = onWalkableGround ? CoyoteTime : CoyoteTimeCounter - Time.deltaTime;
        }

        private void UpdateGroundState()
        {
            if (VerticalVelocity > 0f)
            {
                _ground.SetAirborne();
                return;
            }

            _ground.Sample();
        }

        private void HandleGroundContactChanges()
        {
            if (!_wasGrounded && IsGrounded)
            {
                Landed?.Invoke(VerticalVelocity);
            }
            else if (_wasGrounded && !IsGrounded)
            {
                InheritGroundVelocity();
            }

            _wasGrounded = IsGrounded;
        }

        private void InheritGroundVelocity()
        {
            Vector3 carry = _ground.CarryVelocity;
            if (carry == Vector3.zero) return;

            CurrentMoveVelocity += new Vector3(carry.x, 0f, carry.z);

            if (carry.y > 0f)
            {
                VerticalVelocity += carry.y;
            }

            Vector3 horizontal = CurrentMoveVelocity;
            horizontal.y = 0f;

            AirSpeed = Mathf.Max(AirSpeed, horizontal.magnitude);
        }

        private void ResolveTransitions()
        {
            for (int i = 0; i < MaxTransitionsPerFrame; i++)
            {
                if (!_currentState.TryTransition()) return;
            }
        }

        private void ApplyFinalMovement()
        {
            Vector3 velocity = CurrentMoveVelocity + Vector3.up * VerticalVelocity;

            Controller.Move(velocity * Time.deltaTime);
        }

        private void HandleCeilingCollision()
        {
            if ((Controller.collisionFlags & CollisionFlags.Above) == 0) return;
            if (VerticalVelocity <= 0f) return;

            VerticalVelocity = 0f;
        }

        private void OnValidate()
        {
            if (SlideMinSpeed > SlideMaxSpeed)
            {
                SlideMinSpeed = SlideMaxSpeed;
            }
        }

        private void SnapToGround(Vector3 groundDelta)
        {
            if (!IsGrounded || OnSteepGround || VerticalVelocity > 0f) return;
            if (groundDelta != Vector3.zero) return;

            float snapDistance = _ground.MeasureSnapDistance();
            if (snapDistance <= 0f) return;

            Controller.Move(Vector3.down * snapDistance);
        }
    }
}
