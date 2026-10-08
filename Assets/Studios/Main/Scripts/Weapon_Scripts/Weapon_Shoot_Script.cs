using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
namespace AZE.AdvancedFirstPerson
{
    public class Weapon_Shoot_Script : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public GameObject camera;
        public GameObject weapon;
        public float bulletSpeed;
        public Rigidbody bulletrb;
        public InputAction Click_Shoot;

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
            if (did_click_shoot>0)
            {
                
                bulletPrefab = Instantiate(bulletPrefab, weapon.transform.position, camera.transform.rotation);
                bulletrb = bulletPrefab.GetComponent<Rigidbody>();
                bulletrb.linearVelocity = bulletSpeed * camera.transform.forward;
                
                
                //bulletPrefab.transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                
            }
        
        }

        
        void weaponshoot()
        {
            
            
        }
    }
}
