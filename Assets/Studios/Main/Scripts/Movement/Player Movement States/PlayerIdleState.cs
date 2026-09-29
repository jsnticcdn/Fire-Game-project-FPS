namespace AZE.AdvancedFirstPerson
{
    public class PlayerIdleState : PlayerLocomotionState
    {
        public PlayerIdleState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        protected override float MoveSpeed => 0f;
    }
}
