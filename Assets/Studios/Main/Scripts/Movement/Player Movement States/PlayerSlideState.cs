using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerSlideState : PlayerBaseState
    {
        private Vector3 _slideDirection;
        private float _currentSpeed;

        public PlayerSlideState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        public override void Enter()
        {
            ctx.TargetHeight = ctx.CrouchHeight;

            Vector3 horizontal = ctx.CurrentMoveVelocity;
            horizontal.y = 0f;

            _slideDirection = horizontal.normalized;
            _currentSpeed = horizontal.magnitude;
        }

        public override MovementIntent BuildIntent()
        {
            Steer();
            Accelerate();

            Vector3 velocity = Vector3.ProjectOnPlane(_slideDirection, ctx.GroundNormal).normalized * _currentSpeed;

            return MovementIntent.Immediate(velocity);
        }

        public override bool TryTransition()
        {
            if (TryLeaveGround()) return true;

            if (ctx.JumpRequested && ctx.CanStandUp())
            {
                ctx.InputHandler.CancelCrouch();
                return SwitchTo(factory.Jump);
            }

            if (!ctx.InputHandler.CrouchTriggered && ctx.CanStandUp()) return SwitchTo(factory.ResolveGroundedState());
            if (_currentSpeed <= ctx.CrouchSpeed) return SwitchTo(factory.Crouch);

            return false;
        }

        private void Steer()
        {
            if (!ctx.HasMoveInput) return;

            Vector3 steerTarget = ctx.GetInputWorldVector();
            steerTarget.y = 0f;

            if (steerTarget == Vector3.zero) return;

            float maxRadians = ctx.SlideSteering * Time.deltaTime;
            _slideDirection = Vector3.RotateTowards(_slideDirection, steerTarget.normalized, maxRadians, 0f).normalized;
        }

        private void Accelerate()
        {
            Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, ctx.GroundNormal);
            float steepness = slopeDown.magnitude;

            float acceleration = -ctx.SlideFriction;

            if (steepness > 0.001f)
            {
                float alignment = Vector3.Dot(slopeDown / steepness, _slideDirection);
                acceleration += ctx.SlideSlopeAcceleration * steepness * alignment;
            }

            _currentSpeed = Mathf.Clamp(_currentSpeed + acceleration * Time.deltaTime, 0f, ctx.SlideMaxSpeed);
        }
    }
}
