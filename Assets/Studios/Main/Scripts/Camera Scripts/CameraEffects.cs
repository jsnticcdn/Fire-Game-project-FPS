#if AZE_CINEMACHINE
using UnityEngine;
using Unity.Cinemachine;

namespace AZE.AdvancedFirstPerson
{
    public class CameraEffects : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The movement state machine driving the effects. Found on a parent automatically if left empty.")]
        [SerializeField] private PlayerMovementStateMachine playerMovement;
        [Tooltip("The CinemachineCamera the effects write to. Found on this object automatically if left empty.")]
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [Header("Bob Settings")]
        [SerializeField] private bool useHeadBob = true;
        [Range(0f, 5f)] [SerializeField] private float moveAmplitude = 1.5f;
        [Range(0f, 10f)] [SerializeField] private float moveFrequency = 2.5f;
        [Range(0.1f, 20f)] [SerializeField] private float bobSmoothing = 5f;
        private float idleAmplitude;
        private float idleFrequency;

        [Header("Tilt Settings")]
        [SerializeField] private bool useTilt = true;
        [Range(0f, 10f)] [SerializeField] private float maxTiltAngle = 1.5f;
        [Range(0.1f, 20f)] [SerializeField] private float tiltSmoothing = 8f;

        [Header("Slide Settings")]
        [SerializeField] private bool useSlideTilt = true;
        [Range(0f, 15f)] [SerializeField] private float slideTiltAngle = 4f;

        [Header("FOV Settings")]
        [SerializeField] private bool useFovKick = true;
        [Range(0f, 20f)] [SerializeField] private float fovBoostAmount = 10f;
        [Range(0.1f, 20f)] [SerializeField] private float fovSmoothing = 4f;

        private CinemachineBasicMultiChannelPerlin _cameraNoise;
        private float _baseFOV;

        private void Awake()
        {
            if (cinemachineCamera == null)
                cinemachineCamera = GetComponent<CinemachineCamera>();

            if (playerMovement == null)
                playerMovement = GetComponentInParent<PlayerMovementStateMachine>();

            if (cinemachineCamera == null || playerMovement == null)
            {
                Debug.LogWarning("[AZE] CameraEffects disabled: CinemachineCamera or PlayerMovementStateMachine reference is missing.", this);
                enabled = false;
                return;
            }

            _cameraNoise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (_cameraNoise != null)
            {
                idleAmplitude = _cameraNoise.AmplitudeGain;
                idleFrequency = _cameraNoise.FrequencyGain;
            }

            _baseFOV = cinemachineCamera.Lens.FieldOfView;
        }

        private void Update()
        {
            bool isSliding = playerMovement.CurrentState == playerMovement.States.Slide;
            float speedPercent = playerMovement.CurrentSpeedPercentage;
            float inputX = playerMovement.InputHandler.MoveInput.x;

            HandleBob(isSliding ? 0f : speedPercent);
            HandleTilt(speedPercent, inputX, isSliding);
            HandleFOV(speedPercent);
        }

        private void HandleBob(float speedPercent)
        {
            if (!useHeadBob) return;
            if (_cameraNoise == null) return;

            float targetAmp = Mathf.Lerp(idleAmplitude, moveAmplitude, speedPercent);
            float targetFreq = Mathf.Lerp(idleFrequency, moveFrequency, speedPercent);
            float damp = MotionMath.DampFactor(bobSmoothing, Time.deltaTime);

            _cameraNoise.AmplitudeGain = Mathf.Lerp(_cameraNoise.AmplitudeGain, targetAmp, damp);
            _cameraNoise.FrequencyGain = Mathf.Lerp(_cameraNoise.FrequencyGain, targetFreq, damp);
        }

        private void HandleTilt(float speedPercent, float inputX, bool isSliding)
        {
            if (!useTilt) return;

            float targetTilt = -inputX * maxTiltAngle * speedPercent;

            if (isSliding && useSlideTilt)
            {
                targetTilt += slideTiltAngle;
            }

            var lens = cinemachineCamera.Lens;

            lens.Dutch = Mathf.Lerp(lens.Dutch, targetTilt, MotionMath.DampFactor(tiltSmoothing, Time.deltaTime));

            cinemachineCamera.Lens = lens;
        }

        private void HandleFOV(float speedPercent)
        {
            if (!useFovKick) return;

            float targetFOV = _baseFOV + (fovBoostAmount * speedPercent);

            var lens = cinemachineCamera.Lens;

            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, MotionMath.DampFactor(fovSmoothing, Time.deltaTime));

            cinemachineCamera.Lens = lens;
        }
    }
}
#endif
