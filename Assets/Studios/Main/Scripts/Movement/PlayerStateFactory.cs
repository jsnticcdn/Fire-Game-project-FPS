namespace AZE.AdvancedFirstPerson
{
    public class PlayerStateFactory
    {
        private readonly PlayerMovementStateMachine _context;

        public PlayerBaseState Idle { get; }
        public PlayerBaseState Walk { get; }
        public PlayerBaseState Sprint { get; }
        public PlayerBaseState Crouch { get; }
        public PlayerBaseState Slide { get; }
        public PlayerBaseState Dodge { get; }
        public PlayerBaseState Jump { get; }
        public PlayerBaseState Fall { get; }
        public PlayerBaseState SlopeSlide { get; }

        public PlayerStateFactory(PlayerMovementStateMachine currentContext)
        {
            _context = currentContext;

            Idle = new PlayerIdleState(_context, this);
            Walk = new PlayerWalkState(_context, this);
            Sprint = new PlayerSprintState(_context, this);
            Crouch = new PlayerCrouchState(_context, this);
            Slide = new PlayerSlideState(_context, this);
            Dodge = new PlayerDodgeState(_context, this);
            Jump = new PlayerJumpState(_context, this);
            Fall = new PlayerFallState(_context, this);
            SlopeSlide = new PlayerSlopeSlideState(_context, this);
        }

        public PlayerBaseState ResolveGroundedState()
        {
            if (_context.ShouldSlopeSlide) return SlopeSlide;
            if (_context.SlideRequested) return Slide;
            if (_context.CrouchRequested) return Crouch;
            if (!_context.HasMoveInput) return Idle;
            if (_context.SprintHeld) return Sprint;

            return Walk;
        }
    }
}
