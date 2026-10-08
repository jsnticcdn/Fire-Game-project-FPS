using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
namespace AZE.AdvancedFirstPerson
{
    public class Weapon_Shoot_Script : MonoBehaviour
    {
        public GameObject bulletPrefab;
        
        public GameObject camera;
        public GameObject Aim_Point;
        public float bulletSpeed;
        public Rigidbody bulletrb;
        public InputAction Click_Shoot;
        public Idle_Weapon_Script _idleWeaponScript;
        public float did_click_shoot;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Click_Shoot.Enable();
        }

        // Update is called once per frame
        void Update()
        {
            did_click_shoot = Click_Shoot.ReadValue<float>();
            if (_idleWeaponScript.pickedup == true)
            {
                if (did_click_shoot>0)
                {
                
                    bulletPrefab = Instantiate(bulletPrefab, Aim_Point.transform.position, camera.transform.rotation);
                    bulletrb = bulletPrefab.GetComponent<Rigidbody>();
                    bulletrb.linearVelocity = bulletSpeed * camera.transform.forward;
                    
                
                    //bulletPrefab.transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                
                }
                
            }
            
        }

        void FixedUpdate()
        {
            
        }
        
        

       
    }
}
