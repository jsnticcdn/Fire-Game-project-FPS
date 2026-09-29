namespace AZE.AdvancedFirstPerson
{
    public class PlayerWalkState : PlayerLocomotionState
    {
        public PlayerWalkState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        protected override float MoveSpeed => ctx.WalkSpeed;
    }
}
