namespace AZE.AdvancedFirstPerson
{
    public class PlayerJumpState : PlayerAirborneState
    {
        public PlayerJumpState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        public override void Enter()
        {
            base.Enter();

            ctx.ApplyJumpSpeedPenalty();

            ctx.JumpBufferCounter = 0f;
            ctx.CoyoteTimeCounter = 0f;
            ctx.VerticalVelocity = ctx.JumpForce;
        }

        public override bool TryTransition()
        {
            if (ctx.VerticalVelocity < 0f) return SwitchTo(factory.Fall);

            return false;
        }
    }
}
