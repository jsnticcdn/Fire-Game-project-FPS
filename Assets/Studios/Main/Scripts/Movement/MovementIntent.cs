using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public struct MovementIntent
    {
        public Vector3 TargetVelocity;
        public float Smoothing;
        public bool IsImmediate;

        public static MovementIntent Smoothed(Vector3 targetVelocity, float smoothing)
        {
            return new MovementIntent { TargetVelocity = targetVelocity, Smoothing = smoothing };
        }

        public static MovementIntent Immediate(Vector3 targetVelocity)
        {
            return new MovementIntent { TargetVelocity = targetVelocity, IsImmediate = true };
        }
    }
}
