using UnityEngine;
using System.Collections;
using System;
using Random = System.Random;

namespace AZE.AdvancedFirstPerson
{
    public class MaxCrabScript : MonoBehaviour
    {
       
        public GameObject followpoint;
        public GameObject punchspot;
        private GameObject x;
        public GameObject punch;
        public bool Player_Detected = false;
        public float roam_speed = 4;
        public float Run_Speed = 2;
        public float punch_speed = 8;
        
        public float time;
        public float time1;
        private Random rand = new Random();
        private Vector3 randvec;
        [HideInInspector]
        public GameObject player = null;
        public Rigidbody punchrb;
        private bool[] isfunctionon = {true,false,false};
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Update is called once per frame
        void Update()
        {
            lockfollowpointrotation();
            if (Player_Detected)
            {
                RunFunction();
            }
            else
            {
                isfunctionon[1] = false;
                isfunctionon[2] = false;
            }
            time += Time.deltaTime;
            time1  += Time.deltaTime;
            if (time >= 2)
            {
                randvec = new Vector3(rand.Next(-5,5), 0, rand.Next(-5,5));
                followpoint.transform.localPosition = new Vector3(rand.Next(-5,5), 0, rand.Next(-5,5));
                time = 0;
            }
            if (isfunctionon[0] == true)
            {
                RoamFunction();
                
            }
            if (isfunctionon[1] == true)
            {
                RunFunction();
                
            }

            if (isfunctionon[2] == true)
            {
                attackfunction();
            }
            
            

           
           
            
        
        }
        void RoamFunction()
        {
                transform.position = Vector3.MoveTowards(transform.position, followpoint.transform.position, roam_speed * Time.deltaTime);
                
                if (Player_Detected)
                {
                    isfunctionon[0] = false;
                    isfunctionon[1] = true;
                }
                
            
                
            
            
        }

        void RunFunction()
        {
           
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Run_Speed * Time.deltaTime);
            if (transform.position == player.transform.position)
            {
               
                isfunctionon[1] = false;
                isfunctionon[2] = true;
            }
           
                
            
            
            
        }

        void attackfunction()
        {
            if (time1 >= 4)
            {
                try
                {
                    x = Instantiate(punch, punchspot.transform.position, punch.transform.rotation);
                }
                catch
                {
                    Debug.Log(".");
                }
               
                 punchrb = x.GetComponent<Rigidbody>();
                 punchrb.AddForce(punchspot.transform.forward*punch_speed, ForceMode.Impulse);
                //punch.transform.position = Vector3.MoveTowards(punch.transform.position, realplayer.transform.position, punch_speed * Time.deltaTime);
                // x.transform.position = Vector3.MoveTowards(x.transform.position, player.transform.position, punch_speed * Time.deltaTime);
                 time1 = 0;
                 if (transform.position != player.transform.position)
                 {
                     
                     isfunctionon[2] = false;
                     isfunctionon[1] = true;
                 }
                
            }
        }

        void lockfollowpointrotation()
        {
            followpoint.transform.eulerAngles = new Vector3(0, 0, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                player = other.gameObject;
                Player_Detected = true;
                player = other.gameObject;
            }
        }
    }
}
