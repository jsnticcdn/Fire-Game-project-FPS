using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class GroundSensor
    {
        private const float ProbeOffset = 0.1f;
        private const float MinNormalY = 0.05f;
        private const float SnapTolerance = 0.01f;

        private readonly PlayerMovementStateMachine _context;
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        private Transform _groundTransform;
        private Vector3 _groundPosition;
        private Quaternion _groundRotation;

        public bool IsGrounded { get; private set; }
        public Vector3 Normal { get; private set; } = Vector3.up;
        public float Angle { get; private set; }
        public bool OnSteepGround { get; private set; }
        public Vector3 CarryVelocity { get; private set; }

        public GroundSensor(PlayerMovementStateMachine currentContext)
        {
            _context = currentContext;
        }

        private CharacterController Controller => _context.Controller;

        private Vector3 CapsuleCenter => _context.transform.position + Controller.center;

        public void Sample()
        {
            if (!CastGround(_context.GroundCheckDistance, out RaycastHit hit))
            {
                SetAirborne();
                return;
            }

            SetState(true, RefineNormal(hit, _context.GroundCheckDistance));
            Track(hit.collider.transform);
        }

        public void SetAirborne()
        {
            SetState(false, Vector3.up);
            Track(null);
        }

        public Vector3 Ride()
        {
            if (_groundTransform == null)
            {
                CarryVelocity = Vector3.zero;
                return Vector3.zero;
            }

            Vector3 currentPosition = _groundTransform.position;
            Quaternion currentRotation = _groundTransform.rotation;
            Quaternion rotationDelta = currentRotation * Quaternion.Inverse(_groundRotation);

            Vector3 offset = _context.transform.position - _groundPosition;
            Vector3 delta = currentPosition - _groundPosition + rotationDelta * offset - offset;

            _groundPosition = currentPosition;
            _groundRotation = currentRotation;

            float yawDelta = Mathf.DeltaAngle(0f, rotationDelta.eulerAngles.y);

            if (Mathf.Abs(yawDelta) > 0.0001f)
            {
                _context.transform.Rotate(Vector3.up, yawDelta);
            }

            if (delta != Vector3.zero)
            {
                _context.transform.position += delta;
            }

            CarryVelocity = Time.deltaTime > 0f ? delta / Time.deltaTime : Vector3.zero;

            return delta;
        }

        public float MeasureSnapDistance()
        {
            if (!CastGround(_context.GroundSnapDistance, out RaycastHit hit)) return 0f;
            if (hit.distance <= RestDistance(1f) + SnapTolerance) return 0f;

            Vector3 normal = RefineNormal(hit, _context.GroundSnapDistance);
            if (!IsWalkableNormal(normal)) return 0f;

            float snapDistance = hit.distance - RestDistance(normal.y);
            if (snapDistance <= SnapTolerance) return 0f;

            return snapDistance;
        }

        private void SetState(bool grounded, Vector3 normal)
        {
            IsGrounded = grounded;
            Normal = normal;
            Angle = Vector3.Angle(normal, Vector3.up);
            OnSteepGround = grounded && !IsWalkableAngle(Angle);
        }

        private void Track(Transform ground)
        {
            if (ground == _groundTransform) return;

            _groundTransform = ground;

            if (_groundTransform != null)
            {
                _groundPosition = _groundTransform.position;
                _groundRotation = _groundTransform.rotation;
            }
        }

        private bool IsWalkableAngle(float angle) => angle <= Controller.slopeLimit;

        private bool IsWalkableNormal(Vector3 normal) => IsWalkableAngle(Vector3.Angle(normal, Vector3.up));

        private bool IsValidHit(RaycastHit hit) => hit.collider != Controller && hit.normal.y >= MinNormalY;

        private float ProbeDistance(float reach) => ProbeOffset + reach + Controller.skinWidth;

        private float RestDistance(float normalY) => ProbeOffset + Controller.skinWidth / Mathf.Max(normalY, MinNormalY);

        private bool CastGround(float reach, out RaycastHit closestHit)
        {
            float radius = Controller.radius;
            Vector3 origin = CapsuleCenter + Vector3.up * (radius - Controller.height * 0.5f + ProbeOffset);

            int count = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, _hits, ProbeDistance(reach), _context.GroundLayers, QueryTriggerInteraction.Ignore);

            closestHit = default;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];

                if (hit.distance <= 0f) continue;
                if (hit.distance >= closestDistance) continue;
                if (!IsValidHit(hit)) continue;

                closestDistance = hit.distance;
                closestHit = hit;
            }

            return closestDistance < float.MaxValue;
        }

        private Vector3 RefineNormal(RaycastHit sphereHit, float reach)
        {
            if (TryRaycastNormal(CapsuleCenter, Controller.height * 0.5f + ProbeDistance(reach), out Vector3 axisNormal))
            {
                return axisNormal;
            }

            if (TryRaycastNormal(sphereHit.point + Vector3.up * ProbeOffset, ProbeOffset * 2f, out Vector3 contactNormal))
            {
                return contactNormal;
            }

            return sphereHit.normal;
        }

        private bool TryRaycastNormal(Vector3 origin, float distance, out Vector3 normal)
        {
            normal = Vector3.up;

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, _context.GroundLayers, QueryTriggerInteraction.Ignore)) return false;
            if (!IsValidHit(hit)) return false;

            normal = hit.normal;
            return true;
        }
    }
}
