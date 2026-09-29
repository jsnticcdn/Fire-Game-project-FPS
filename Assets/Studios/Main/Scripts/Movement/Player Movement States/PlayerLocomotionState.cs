namespace AZE.AdvancedFirstPerson
{
    public abstract class PlayerLocomotionState : PlayerBaseState
    {
        protected PlayerLocomotionState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        public override bool TryTransition()
        {
            if (TryLeaveGround()) return true;
            if (ctx.JumpRequested) return SwitchTo(factory.Jump);
            if (ctx.SlideRequested) return SwitchTo(factory.Slide);
            if (ctx.CrouchRequested) return SwitchTo(factory.Crouch);
            if (ctx.TryConsumeDodge()) return SwitchTo(factory.Dodge);

            PlayerBaseState nextState = factory.ResolveGroundedState();
            if (nextState != this) return SwitchTo(nextState);

            return false;
        }
    }
}
