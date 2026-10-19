using NUnit.Framework.Constraints;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class Idle_Weapon_Script : MonoBehaviour
    {
        public InputAction takeGun;
        public float istakingweapon = 0;
        
        public InputAction unequip;
        public float isremovingweapon = 0;
        
        public CinemachineCamera camera;
        public bool isparent = false;
        public Multi_Weapon_Check Weapon_Cap_Check;
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
            isremovingweapon = unequip.ReadValue<float>();
            if (isparent)
            {
                transform.localPosition = gunPosition;
                transform.localScale = floorScale * equippedScaleMultiplier;
            }

            else if (isremovingweapon > 0)
            {
                if (pickedup)
                {
                    transform.parent = null;
                    pickedup = false;
                }
               
            }
            
        }

        private void OnTriggerStay(Collider other)
        {

            if (istakingweapon > 0)
            {
                if (Weapon_Cap_Check.Weapon_Count == 0)
                {
                    isparent = true;
                    transform.SetParent(camera.transform);
                    transform.localPosition = gunPosition;
                    transform.localRotation = Quaternion.identity;
                    pickedup = true;
                    Weapon_Cap_Check.Weapon_Count = 1;
                    
                }
               
                
            }
           
           
           
               

            
            
        }
    }
}
