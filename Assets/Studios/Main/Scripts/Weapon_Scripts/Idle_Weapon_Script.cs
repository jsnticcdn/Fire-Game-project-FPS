using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class Idle_Weapon_Script : MonoBehaviour
    {
        public InputAction take_gun;
<<<<<<< HEAD
        public CinemachineCamera camera;
        public GameObject player;
=======
        public CinemachineCamera player;

>>>>>>> e10f9595a9d3508e65c74089214b59edfbe71222
        public float istakingweapon = 0;

        public bool isparent = false;

        [Header("Gun Position")]
        public Vector3 gunPosition = new Vector3(2.5f, 1f, 2f);

        [Header("Wall Detection")]
        public float wallCheckDistance = 0.5f;
        public float wallOffset = 0.05f;
        public LayerMask wallLayers;

        void Start()
        {
            take_gun.Enable();
        }

        void Update()
        {
            istakingweapon = take_gun.ReadValue<float>();

            if (isparent)
            {
<<<<<<< HEAD
                //transform.position = player.transform.position + new Vector3(-0.5f, 1.5f, -0.5f);
                transform.rotation = player.transform.rotation;
                transform.position = camera.transform.position + new Vector3(1,0,1);
                transform.parent = player.transform;
            }

        } 
=======
                // Make sure the gun stays attached to the camera
                if (transform.parent != player.transform)
                {
                    transform.SetParent(player.transform);
                }

                // Default gun position
                Vector3 targetPosition = gunPosition;

                // Check if there is a wall between the camera and the gun
                Vector3 cameraPosition = player.transform.position;

                Vector3 direction = (player.transform.TransformPoint(gunPosition) - cameraPosition).normalized;

                float distance = Vector3.Distance(
                    cameraPosition,
                    player.transform.TransformPoint(gunPosition)
                );

                if (Physics.Raycast(
                    cameraPosition,
                    direction,
                    out RaycastHit hit,
                    distance + wallCheckDistance,
                    wallLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    // Convert the wall hit position into camera local space
                    Vector3 localHitPoint =
                        player.transform.InverseTransformPoint(hit.point);

                    // Move the gun toward the camera
                    targetPosition.z = localHitPoint.z + wallOffset;

                    // Don't let the gun move farther away than its normal position
                    targetPosition.z = Mathf.Max(
                        targetPosition.z,
                        gunPosition.z
                    );
                }

                transform.localPosition = targetPosition;

                // Match the camera's rotation
                transform.localRotation = Quaternion.identity;
            }
        }
>>>>>>> e10f9595a9d3508e65c74089214b59edfbe71222

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (istakingweapon > 0)
                {
                    isparent = true;

                    // Parent directly to the camera
                    transform.SetParent(player.transform);

                    // Set initial position
                    transform.localPosition = gunPosition;

                    // Match camera rotation
                    transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
