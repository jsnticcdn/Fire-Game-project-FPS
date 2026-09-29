namespace AZE.AdvancedFirstPerson
{
    public class PlayerSprintState : PlayerLocomotionState
    {
        public PlayerSprintState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        protected override float MoveSpeed => ctx.RunSpeed;
    }
}
