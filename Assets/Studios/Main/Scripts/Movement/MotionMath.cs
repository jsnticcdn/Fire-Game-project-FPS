using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public static class MotionMath
    {
        public static float DampFactor(float smoothing, float deltaTime)
        {
            return 1f - Mathf.Exp(-smoothing * deltaTime);
        }
    }
}
