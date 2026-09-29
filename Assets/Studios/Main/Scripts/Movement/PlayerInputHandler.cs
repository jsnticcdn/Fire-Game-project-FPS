using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public enum CrouchInputMode
        {
            Toggle,
            Hold
        }

        private const float MaxDoubleTapTime = 0.2f;
        private const float DodgeBufferTime = 0.2f;
        private const float DoubleTapDirectionDot = 0.8f;

        [Header("Input Actions")]
        [Tooltip("Vector2 action that drives movement.")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Vector2 action that drives the camera look.")]
        [SerializeField] private InputActionReference lookAction;
        [Tooltip("Button action for jumping.")]
        [SerializeField] private InputActionReference jumpAction;
        [Tooltip("Button action held to sprint.")]
        [SerializeField] private InputActionReference sprintAction;
        [Tooltip("Button action for crouching, read as toggle or hold depending on Crouch Mode.")]
        [SerializeField] private InputActionReference crouchAction;
        [Tooltip("Vector2 action whose double-tap triggers the dodge.")]
        [SerializeField] private InputActionReference dodgeAction;

        [Header("Behaviour")]
        [Tooltip("Toggle: press to crouch, press again to stand. Hold: crouch only while the button is held.")]
        [SerializeField] private CrouchInputMode crouchMode = CrouchInputMode.Toggle;

        public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
        public Vector2 LookInput => lookAction.action.ReadValue<Vector2>();
        public bool JumpTriggered => jumpAction.action.WasPressedThisFrame();
        public bool SprintPressed => sprintAction.action.IsPressed();
        public bool CrouchTriggered => crouchMode == CrouchInputMode.Hold ? crouchAction.action.IsPressed() : _crouchToggled;
        public bool DodgeTriggered => _dodgeBuffered && Time.time - _dodgeBufferedTime <= DodgeBufferTime;
        public Vector2 DodgeDirection { get; private set; }

        public InputDevice ActiveLookDevice => lookAction.action.activeControl?.device;

        public bool HasActionsAssigned =>
            HasAction(moveAction) && HasAction(lookAction) && HasAction(jumpAction) &&
            HasAction(sprintAction) && HasAction(crouchAction) && HasAction(dodgeAction);

        private bool _crouchToggled;
        private bool _dodgeBuffered;
        private float _dodgeBufferedTime;
        private int _dodgeTapCount;
        private float _lastTapTime;
        private Vector2 _lastDodgeInput;

        private void OnEnable()
        {
            if (!HasActionsAssigned)
            {
                Debug.LogError("[AZE] PlayerInputHandler: all Input Action References must be assigned. Drag the actions from Inputs/InputSystem_Actions_Movement into the Inspector.", this);
                enabled = false;
                return;
            }

            moveAction.action.Enable();
            lookAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
            crouchAction.action.Enable();
            dodgeAction.action.Enable();

            crouchAction.action.performed += HandleCrouchInput;
            dodgeAction.action.started += HandleDodgeInput;
            dodgeAction.action.performed += HandleDodgeInput;
        }

        private void OnDisable()
        {
            if (!HasActionsAssigned) return;

            crouchAction.action.performed -= HandleCrouchInput;
            dodgeAction.action.started -= HandleDodgeInput;
            dodgeAction.action.performed -= HandleDodgeInput;

            moveAction.action.Disable();
            lookAction.action.Disable();
            jumpAction.action.Disable();
            sprintAction.action.Disable();
            crouchAction.action.Disable();
            dodgeAction.action.Disable();
        }

        public void ConsumeDodgeBuffer() => _dodgeBuffered = false;

        public void CancelCrouch() => _crouchToggled = false;

        private static bool HasAction(InputActionReference reference) => reference != null && reference.action != null;

        private void HandleCrouchInput(InputAction.CallbackContext context)
        {
            if (crouchMode == CrouchInputMode.Toggle)
            {
                _crouchToggled = !_crouchToggled;
            }
        }

        private void HandleDodgeInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Vector2 currentInput = context.ReadValue<Vector2>();
                bool isConsecutiveTap = IsSameDirection(_lastDodgeInput, currentInput) && Time.time - _lastTapTime <= MaxDoubleTapTime;

                _dodgeTapCount = isConsecutiveTap ? _dodgeTapCount + 1 : 1;
                _lastTapTime = Time.time;
                _lastDodgeInput = currentInput;
                DodgeDirection = currentInput;
            }

            if (context.performed && _dodgeTapCount >= 2)
            {
                _dodgeTapCount = 0;
                _dodgeBuffered = true;
                _dodgeBufferedTime = Time.time;
            }
        }

        private static bool IsSameDirection(Vector2 previous, Vector2 current)
        {
            if (previous == Vector2.zero || current == Vector2.zero) return false;

            return Vector2.Dot(previous.normalized, current.normalized) >= DoubleTapDirectionDot;
        }
    }
}
