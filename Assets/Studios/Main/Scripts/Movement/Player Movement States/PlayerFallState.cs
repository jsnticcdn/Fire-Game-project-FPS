namespace AZE.AdvancedFirstPerson
{
    public class PlayerFallState : PlayerAirborneState
    {
        public PlayerFallState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        public override bool TryTransition()
        {
            if (ctx.JumpRequested) return SwitchTo(factory.Jump);
            if (!ctx.IsGrounded) return false;

            return SwitchTo(factory.ResolveGroundedState());
        }
    }
}
