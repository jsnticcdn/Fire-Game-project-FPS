using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class CapsuleHeightController
    {
        private const float CeilingProbeRadiusScale = 0.7f;
        private const float CeilingProbeOffset = 0.05f;
        private const float HeightSnapThreshold = 0.05f;

        private readonly PlayerMovementStateMachine _context;

        public float StandingHeight { get; }
        public float CrouchHeight { get; }
        public float TargetHeight { get; set; }

        public CapsuleHeightController(PlayerMovementStateMachine currentContext)
        {
            _context = currentContext;

            StandingHeight = Controller.height;
            CrouchHeight = StandingHeight * currentContext.CrouchHeightRatio;
            TargetHeight = StandingHeight;
        }

        private CharacterController Controller => _context.Controller;

        public bool CanStandUp()
        {
            float radius = Controller.radius * CeilingProbeRadiusScale;
            Vector3 castOrigin = _context.transform.position + Vector3.up * (CrouchHeight - radius + CeilingProbeOffset);

            return !Physics.SphereCast(castOrigin, radius, Vector3.up, out _, StandingHeight - CrouchHeight, _context.CeilingLayers, QueryTriggerInteraction.Ignore);
        }

        public void Interpolate()
        {
            float currentHeight = Controller.height;
            if (currentHeight == TargetHeight) return;
            if (TargetHeight > currentHeight && !CanStandUp()) return;

            Transform cameraTransform = _context.CameraTransform;
            Vector3 cameraPosition = cameraTransform.localPosition;
            float targetCameraY = TargetHeight - _context.CameraOffset;

            if (Mathf.Abs(currentHeight - TargetHeight) < HeightSnapThreshold)
            {
                ApplyHeight(TargetHeight);
                cameraPosition.y = targetCameraY;
            }
            else
            {
                float step = MotionMath.DampFactor(_context.CrouchTransitionSpeed, Time.deltaTime);

                ApplyHeight(Mathf.Lerp(currentHeight, TargetHeight, step));
                cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraY, step);
            }

            cameraTransform.localPosition = cameraPosition;
        }

        private void ApplyHeight(float height)
        {
            Controller.height = height;
            Controller.center = Vector3.up * (height * 0.5f);
        }
    }
}
