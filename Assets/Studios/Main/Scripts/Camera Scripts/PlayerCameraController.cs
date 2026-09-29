using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerCameraController : MonoBehaviour
    {
        private const float MouseMultiplier = 0.01f;
        private const float LookInputDeadzoneSqr = 0.0001f;

        [Header("Settings")]
        [Tooltip("Horizontal look speed.")]
        [Range(0f, 100f)] public float SensitivityX = 20f;
        [Tooltip("Vertical look speed.")]
        [Range(0f, 100f)] public float SensitivityY = 15f;
        [Tooltip("Invert the vertical look axis.")]
        public bool InvertY = false;
        [Tooltip("How far the player can look up, in degrees.")]
        [Range(0f, 90f)] public float MaxLookUpAngle = 90f;
        [Tooltip("How far the player can look down, in degrees.")]
        [Range(0f, 90f)] public float MaxLookDownAngle = 75f;
        [Tooltip("Lock and hide the cursor on Awake. Call SetCursorLocked to change it at runtime.")]
        public bool LockCursorOnStart = true;

        [Header("References")]
        [Tooltip("The camera pivot rotated by vertical look. Must be a child of this transform.")]
        public Transform CameraTransform;
        [Tooltip("The input handler that supplies the look input.")]
        [SerializeField] private PlayerInputHandler inputHandler;

        private float _cameraPitch;

        private void Awake()
        {
            if (CameraTransform == null || inputHandler == null)
            {
                Debug.LogError("[AZE] PlayerCameraController: CameraTransform and Input Handler must be assigned.", this);
                enabled = false;
                return;
            }

            if (LockCursorOnStart)
            {
                SetCursorLocked(true);
            }
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void LateUpdate()
        {
            HandleRotation();
        }

        private void HandleRotation()
        {
            Vector2 lookInput = inputHandler.LookInput;
            if (lookInput.sqrMagnitude < LookInputDeadzoneSqr) return;

            float multiplier = inputHandler.ActiveLookDevice is Mouse ? MouseMultiplier : Time.deltaTime;

            float yaw = lookInput.x * SensitivityX * multiplier;
            transform.Rotate(Vector3.up * yaw);

            float pitchDelta = lookInput.y * SensitivityY * multiplier;
            _cameraPitch += InvertY ? pitchDelta : -pitchDelta;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -MaxLookUpAngle, MaxLookDownAngle);

            CameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }
    }
}
