using System;
using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class Camera_Facer : MonoBehaviour
    {
        public bool Lock_Y_Rotation = false;
        private bool Player_Detected = false;
        private GameObject player;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            
            if (Lock_Y_Rotation)
            {
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
          
            
            if (transform.rotation.eulerAngles.x < -20)
            {
                transform.eulerAngles = new Vector3(-20, transform.eulerAngles.y, transform.eulerAngles.z);
                
            }
           
        
        }

        private void FixedUpdate()
        {
            if (Player_Detected)
            {
                transform.LookAt(player.transform, Vector3.up);
            }
        }


        private void OnTriggerStay(Collider other)
        {
            
                if (other.gameObject.CompareTag("Actual_Player"))
                {
                   
                    player = other.gameObject;
                    Player_Detected = true;
                    transform.LookAt(player.transform, Vector3.up);
                }
                else
                {
                   
                    Player_Detected = false;
                    player = null;
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                }
                
            
           
        }
    }
}
