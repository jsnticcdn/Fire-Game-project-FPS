using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        private const float GizmoRadius = 0.15f;

        public enum PathMode
        {
            PingPong,
            Loop,
            Once
        }

        [Header("Path")]
        [Tooltip("PingPong retraces the path, Loop closes the circuit back to the start, Once stops at the last waypoint.")]
        public PathMode Mode = PathMode.PingPong;
        [Tooltip("Offsets from the platform's starting position, not world coordinates. The starting position is already the first point of the path.")]
        public Vector3[] Waypoints = { new Vector3(0f, 3f, 0f) };

        [Header("Motion")]
        [Tooltip("Travel speed in units per second. With easing enabled this is the average speed of each leg.")]
        [Range(0.1f, 20f)] public float Speed = 2f;
        [Tooltip("Pause on arrival at every waypoint, in seconds.")]
        [Range(0f, 10f)] public float WaitTime = 0.5f;
        [Tooltip("Accelerate and decelerate each leg instead of moving at constant speed.")]
        public bool UseEasing = true;

        [Header("Control")]
        [Tooltip("Start moving on Awake. Turn off to drive it from script with StartMoving / StopMoving / ResetToStart.")]
        public bool AutoStart = true;

        public Vector3 Velocity { get; private set; }
        public bool IsMoving { get; private set; }

        private Vector3[] _points;
        private Vector3 _origin;
        private int _fromIndex;
        private int _toIndex;
        private int _direction = 1;
        private float _progress;
        private float _waitTimer;

        private bool HasPath => _points.Length > 1;

        private void Awake()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.None;

            _origin = transform.position;
            BuildPath();
            ResetToStart();

            if (AutoStart)
            {
                StartMoving();
            }
        }

        private void Update()
        {
            if (!IsMoving)
            {
                Velocity = Vector3.zero;
                return;
            }

            Vector3 previousPosition = transform.position;
            AdvanceAlongPath();

            Velocity = Time.deltaTime > 0f ? (transform.position - previousPosition) / Time.deltaTime : Vector3.zero;
        }

        public void StartMoving()
        {
            if (HasPath) IsMoving = true;
        }

        public void StopMoving()
        {
            IsMoving = false;
        }

        public void ResetToStart()
        {
            transform.position = _points[0];

            _fromIndex = 0;
            _toIndex = HasPath ? 1 : 0;
            _direction = 1;
            _progress = 0f;
            _waitTimer = 0f;
        }

        private void BuildPath()
        {
            int waypointCount = Waypoints != null ? Waypoints.Length : 0;
            _points = new Vector3[waypointCount + 1];
            _points[0] = _origin;

            for (int i = 0; i < waypointCount; i++)
            {
                _points[i + 1] = WorldWaypoint(_origin, i);
            }
        }

        private Vector3 WorldWaypoint(Vector3 origin, int index) => origin + Waypoints[index];

        private void AdvanceAlongPath()
        {
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.deltaTime;
                return;
            }

            Vector3 from = _points[_fromIndex];
            Vector3 to = _points[_toIndex];
            float distance = Vector3.Distance(from, to);

            if (distance <= Mathf.Epsilon)
            {
                ArriveAtTarget(to);
                return;
            }

            _progress += Speed * Time.deltaTime / distance;

            if (_progress >= 1f)
            {
                ArriveAtTarget(to);
                return;
            }

            float t = UseEasing ? Mathf.SmoothStep(0f, 1f, _progress) : _progress;
            transform.position = Vector3.Lerp(from, to, t);
        }

        private void ArriveAtTarget(Vector3 target)
        {
            transform.position = target;

            _progress = 0f;
            _waitTimer = WaitTime;
            _fromIndex = _toIndex;

            switch (Mode)
            {
                case PathMode.Loop:
                    _toIndex = (_toIndex + 1) % _points.Length;
                    break;

                case PathMode.PingPong:
                    if (_toIndex + _direction < 0 || _toIndex + _direction >= _points.Length)
                    {
                        _direction = -_direction;
                    }
                    _toIndex += _direction;
                    break;

                case PathMode.Once:
                    if (_toIndex + 1 >= _points.Length)
                    {
                        IsMoving = false;
                    }
                    else
                    {
                        _toIndex++;
                    }
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Waypoints == null || Waypoints.Length == 0) return;

            Vector3 origin = Application.isPlaying ? _origin : transform.position;
            Vector3 previous = origin;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, GizmoRadius);

            for (int i = 0; i < Waypoints.Length; i++)
            {
                Vector3 point = WorldWaypoint(origin, i);

                Gizmos.DrawLine(previous, point);
                Gizmos.DrawWireSphere(point, GizmoRadius);

                previous = point;
            }

            if (Mode == PathMode.Loop)
            {
                Gizmos.DrawLine(previous, origin);
            }
        }
    }
}
