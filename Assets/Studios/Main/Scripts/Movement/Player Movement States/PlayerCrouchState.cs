namespace AZE.AdvancedFirstPerson
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        protected override float MoveSpeed => ctx.CrouchSpeed;

        public override void Enter()
        {
            ctx.TargetHeight = ctx.CrouchHeight;
        }

        public override bool TryTransition()
        {
            if (TryLeaveGround()) return true;

            if (ctx.JumpRequested && ctx.CanStandUp())
            {
                ctx.InputHandler.CancelCrouch();
                return SwitchTo(factory.Jump);
            }

            if (ctx.InputHandler.CrouchTriggered) return false;
            if (!ctx.CanStandUp()) return false;

            return SwitchTo(factory.ResolveGroundedState());
        }
    }
}
