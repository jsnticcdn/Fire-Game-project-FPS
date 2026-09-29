namespace AZE.AdvancedFirstPerson
{
    public abstract class PlayerBaseState
    {
        protected PlayerMovementStateMachine ctx;
        protected PlayerStateFactory factory;

        protected PlayerBaseState(PlayerMovementStateMachine currentContext, PlayerStateFactory playerStateFactory)
        {
            ctx = currentContext;
            factory = playerStateFactory;
        }

        protected virtual float MoveSpeed => ctx.WalkSpeed;

        public virtual void Enter()
        {
            ctx.TargetHeight = ctx.StandingHeight;
        }

        public virtual MovementIntent BuildIntent()
        {
            return MovementIntent.Smoothed(ctx.BuildGroundedVelocity(MoveSpeed), ctx.MovementSmoothing);
        }

        public virtual void Exit() { }

        public abstract bool TryTransition();

        protected bool TryLeaveGround()
        {
            if (!ctx.IsGrounded) return SwitchTo(factory.Fall);
            if (ctx.ShouldSlopeSlide) return SwitchTo(factory.SlopeSlide);

            return false;
        }

        protected bool SwitchTo(PlayerBaseState nextState)
        {
            ctx.SwitchState(nextState);
            return true;
        }
    }
}
