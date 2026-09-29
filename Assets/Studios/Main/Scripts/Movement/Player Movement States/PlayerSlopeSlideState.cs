using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerSlopeSlideState : PlayerBaseState
    {
        public PlayerSlopeSlideState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        private Vector3 FallLine => Vector3.ProjectOnPlane(Vector3.down, ctx.GroundNormal).normalized;

        public override MovementIntent BuildIntent()
        {
            ctx.ConstrainVelocityToGround();

            Vector3 steering = Vector3.ProjectOnPlane(ctx.GetInputWorldVector(), ctx.GroundNormal).normalized;
            float steepness = Mathf.InverseLerp(ctx.Controller.slopeLimit, 90f, ctx.GroundAngle);

            Vector3 targetVelocity = FallLine * (ctx.SlopeSlideSpeed * Mathf.Lerp(0.5f, 1f, steepness))
                                   + steering * (ctx.WalkSpeed * ctx.SlopeSlideControl);

            return MovementIntent.Smoothed(targetVelocity, ctx.MovementSmoothing);
        }

        public override bool TryTransition()
        {
            if (!ctx.IsGrounded) return SwitchTo(factory.Fall);
            if (!ctx.ShouldSlopeSlide) return SwitchTo(factory.ResolveGroundedState());

            return false;
        }
    }
}
