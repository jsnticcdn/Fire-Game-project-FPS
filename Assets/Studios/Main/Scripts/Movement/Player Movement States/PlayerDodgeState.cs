using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerDodgeState : PlayerBaseState
    {
        private float _elapsed;
        private Vector3 _dodgeDirection;

        public PlayerDodgeState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        public override void Enter()
        {
            base.Enter();

            _elapsed = 0f;
            _dodgeDirection = ResolveDodgeDirection();
            ctx.LastDodgeTime = Time.time;
        }

        public override MovementIntent BuildIntent()
        {
            float dodgeSpeed = Mathf.Lerp(ctx.DodgeSpeed, 0f, _elapsed / ctx.DodgeDuration);
            _elapsed += Time.deltaTime;

            return MovementIntent.Immediate(_dodgeDirection * dodgeSpeed);
        }

        public override bool TryTransition()
        {
            if (_elapsed < ctx.DodgeDuration) return false;

            return SwitchTo(factory.ResolveGroundedState());
        }

        private Vector3 ResolveDodgeDirection()
        {
            Vector2 input = ctx.InputHandler.DodgeDirection;
            Vector3 localDir = Vector3.zero;

            if (input.y < 0f) localDir += Vector3.back;
            if (input.y > 0f) localDir += Vector3.forward;
            if (input.x < 0f) localDir += Vector3.left;
            if (input.x > 0f) localDir += Vector3.right;

            if (localDir == Vector3.zero) localDir = Vector3.back;

            Vector3 worldDir = ctx.CameraTransform.TransformDirection(localDir);
            worldDir.y = 0f;

            return worldDir.normalized;
        }
    }
}
