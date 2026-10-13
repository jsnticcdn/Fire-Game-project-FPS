using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
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
        private Quaternion random_Angle;
        public Idle_Weapon_Script idlescript;

        private float fire_rate = 1.0f;

        private float timey = 0;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Click_Shoot.Enable();
            
        }

        // Update is called once per frame
        void Update()
        {
            timey += Time.deltaTime;
            did_click_shoot = Click_Shoot.ReadValue<float>();
            if (timey > fire_rate)
            {
                if (did_click_shoot>0)
                {
                    if (idlescript.pickedup == true)
                    {

                        for (int i = 0; i < 5; i++)
                        {
                            random_Angle = new Quaternion(Random.Range(0.0f, 360.0f), Random.Range(0.0f,360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360));
                            GameObject x =Instantiate(bulletPrefab, weapon.transform.position, camera.transform.rotation * random_Angle);
                            bulletrb = x.GetComponent<Rigidbody>();
                            bulletrb.linearVelocity = bulletSpeed * camera.transform.forward ;
                            
                            
                        }
                        timey = 0;

                    }
     
                }
                
                
            }
            
            
            else
            {
                return;
            }
        
        }
       
       

        
    }
}
