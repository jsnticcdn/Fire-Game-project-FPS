using NUnit.Framework.Constraints;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class Idle_Weapon_Script : MonoBehaviour
    {
        public InputAction takeGun;
        public CinemachineCamera camera;
        public float istakingweapon = 0;
        public bool isparent = false;

        [Header("Gun Position")]
        public Vector3 gunPosition = new Vector3(1.70f, -0.8f, 2.5f);
        public float equippedScaleMultiplier = 1f;
        public bool pickedup = false;
        Vector3 floorScale;

        void Start()
        {
            takeGun.Enable();
            floorScale = transform.localScale;
        }

        void Update()
        {
            istakingweapon = takeGun.ReadValue<float>();

            if (isparent)
            {
                transform.localPosition = gunPosition;
                transform.localScale = floorScale * equippedScaleMultiplier;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isparent)
                return;
            if (!other.CompareTag("Player"))
                return;
            if (istakingweapon <= 0)
                return;

            isparent = true;
            transform.SetParent(camera.transform);
            transform.localPosition = gunPosition;
            transform.localRotation = Quaternion.identity;
            pickedup = true;
        }
    }
}
