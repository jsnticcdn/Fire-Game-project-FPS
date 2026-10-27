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

        public float[] minvals = { 0, 0, 0, 0 };
        public float[] maxvals = { 0, 0, 0, 0 };
        
        public Quaternion Angle_Spread;
        
        
        private float did_click_shoot;
        public float bulletSpeed;
        public float fire_rate = 1.0f;
        private float bullet_delay = 0;
        public int bullets_per_shot = 5;
        
        private float time = 0;
        public float recoil_force = 1.0f;
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
            
            
 
        }

        void FixedUpdate()
        {
            if (bullet_delay > fire_rate)
            {
                if (did_click_shoot > 0)
                {
                    if (idlescript.pickedup == true)
                    {

                        Shoot();
                        bullet_delay = 0;

                    }
     
                }
                
                
            }
            
        }
        
        void Shoot()
        {
            
            for (int i = 0; i < bullets_per_shot; i++)
            {
                
                Spread_Bullet_Angles();
                GameObject x =Instantiate(bulletPrefab, weapon.transform.position, camera.transform.rotation * Angle_Spread);
                Spread_Bullet_Angles();
                x.transform.Rotate(Angle_Spread.eulerAngles);
                
                
                bulletrb = x.GetComponent<Rigidbody>();
                bulletrb.linearVelocity = bulletSpeed * weapon.transform.forward ;
                            
                            
            }

            bullet_delay = 0;
            
        }

        void Spread_Bullet_Angles()
        {
            Angle_Spread = new Quaternion(Random.Range(minvals[0],maxvals[0]),Random.Range(minvals[1],maxvals[1]),Random.Range(minvals[2],maxvals[02]),Random.Range(minvals[3],maxvals[3]));
        }


    }
}
