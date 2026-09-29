using UnityEngine;
using UnityEngine.Serialization;

namespace AZE.AdvancedFirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterPushInteraction : MonoBehaviour
    {
        private const float DownwardHitThreshold = -0.3f;

        [Header("Push Settings")]
        [Tooltip("Base strength of the push, in m/s.")]
        [FormerlySerializedAs("pushPower")] [Range(0f, 20f)] public float PushPower = 2f;
        [Tooltip("Reference mass in kg: objects at or below it receive the full push, heavier ones proportionally less. 0 pushes everything at full strength.")]
        [FormerlySerializedAs("weightBasedPush")] [Range(0f, 100f)] public float WeightBasedPush = 1f;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic) return;
            if (hit.moveDirection.y < DownwardHitThreshold) return;

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            float massScale = WeightBasedPush > 0f ? Mathf.Clamp01(WeightBasedPush / body.mass) : 1f;

            Vector3 velocity = pushDirection * (PushPower * massScale);
            velocity.y = body.linearVelocity.y;

            body.linearVelocity = velocity;
        }
    }
}
