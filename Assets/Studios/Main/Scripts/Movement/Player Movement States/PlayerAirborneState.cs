namespace AZE.AdvancedFirstPerson
{
    public abstract class PlayerAirborneState : PlayerBaseState
    {
        protected PlayerAirborneState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        protected override float MoveSpeed => ctx.AirSpeed;

        public override void Enter()
        {
            ctx.TargetHeight = ctx.CrouchRequested ? ctx.CrouchHeight : ctx.StandingHeight;
        }

        public override MovementIntent BuildIntent()
        {
            return MovementIntent.Smoothed(ctx.BuildAirborneVelocity(MoveSpeed), ctx.AirSmoothing);
        }
    }
}
