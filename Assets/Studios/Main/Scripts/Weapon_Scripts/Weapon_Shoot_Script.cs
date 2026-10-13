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
        public float Bullet_lifespan = 1;
        public float did_click_shoot;

        public Idle_Weapon_Script idlescript;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Click_Shoot.Enable();
            bulletPrefab = Instantiate(bulletPrefab, weapon.transform.position, camera.transform.rotation);
        }

        // Update is called once per frame
        void Update()
        {
            did_click_shoot = Click_Shoot.ReadValue<float>();
            if (did_click_shoot>0)
            {
                if (idlescript.pickedup == true)
                {
                    bulletPrefab = Instantiate(bulletPrefab, weapon.transform.position, camera.transform.rotation);
                    bulletrb = bulletPrefab.GetComponent<Rigidbody>();
                    bulletrb.linearVelocity = bulletSpeed * camera.transform.forward;
                    
                   StartCoroutine(DestroyBullet());
                   
                }
                
                
                

                //bulletPrefab.transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);

            }
            
            else
            {
                return;
            }
        
        }
        private IEnumerator DestroyBullet()
        {
            
            yield return new WaitForSeconds(2f);
            Destroy(GameObject.FindGameObjectWithTag("Bullet"));
            
        }
       

        
        void weaponshoot()
        {
            
            
        }
    }
}
