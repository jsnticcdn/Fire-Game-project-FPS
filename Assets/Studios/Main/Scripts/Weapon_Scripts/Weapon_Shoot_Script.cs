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
        public Rigidbody bulletrb;
        public InputAction Click_Shoot;
        public Idle_Weapon_Script idlescript;
        
        
        public Quaternion Angle_Spread;
        private float did_click_shoot;
        public float fire_rate = 1.0f;
        public float bulletSpeed;
        private float bullet_delay = 0;
        public float bullets_per_shot = 5;
        public float[] Angle_Spread_Range = { 0, 360, 0,360,0,360,0,360 };
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Click_Shoot.Enable();
            
        }

        // Update is called once per frame
        void Update()
        {
            bullet_delay += Time.deltaTime;
            did_click_shoot = Click_Shoot.ReadValue<float>();
            if (bullet_delay > fire_rate)
            {
                if (did_click_shoot>0)
                {
                    if (idlescript.pickedup == true)
                    {

                        for (int i = 0; i < bullets_per_shot; i++)
                        {
                            Angle_Spread = new Quaternion(
                                Random.Range(Angle_Spread_Range[0], Angle_Spread_Range[1]), 
                                Random.Range(Angle_Spread_Range[2], Angle_Spread_Range[3]),
                                Random.Range(Angle_Spread_Range[4], Angle_Spread_Range[5]), 
                                Random.Range(Angle_Spread_Range[6], Angle_Spread_Range[7]));
                            
                            GameObject x =Instantiate(bulletPrefab, weapon.transform.position, camera.transform.rotation * Angle_Spread);
                            bulletrb = x.GetComponent<Rigidbody>();
                            bulletrb.linearVelocity = bulletSpeed * camera.transform.forward ;
                            
                            
                        }
                        bullet_delay = 0;

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
