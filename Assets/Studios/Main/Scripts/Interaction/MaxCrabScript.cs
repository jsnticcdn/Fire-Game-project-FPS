using UnityEngine;
using System.Collections;
using System;
using Random = System.Random;

namespace AZE.AdvancedFirstPerson
{
    public class MaxCrabScript : MonoBehaviour
    {
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
        private Vector3 randvec =  new Vector3();
        [HideInInspector]
        private GameObject player = null;

        private GameObject ActualPlayer = null;
        public Rigidbody punchrb;
        private bool[] functionpath = {true,false,false};
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            
            
            time += Time.deltaTime;
            time1  += Time.deltaTime;

            if (time >= 2)
            {
                randvec = new Vector3(rand.Next(-10, 10), 0, rand.Next(-10, 10));
                time = 0;

            }
            if (functionpath[0] == true)
            {
                RoamFunction();
                
            }
            if (functionpath[1] == true)
            {
                RunFunction();
                
            }
            if (functionpath[2] == true)
            {
                //attackfunction();
                StartCoroutine(attackf());
            }
            
            

           
           
            
        
        }
        void RoamFunction()
        {
                
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition+randvec, roam_speed * Time.deltaTime);
                if (Player_Detected)
                {
                    functionpath[0] = false;
                    
                    functionpath[1] = true;
                }
                
            
                
          
            
        }

        void RunFunction()
        {
           
            //transform.localPosition = Vector3.MoveTowards(transform.position, player.transform.position, Run_Speed * Time.deltaTime);
            transform.localPosition = Vector3.MoveTowards(transform.position, player.transform.position, Run_Speed * Time.deltaTime);
            if (transform.localPosition.x.Equals(player.transform.position.x))
            {
               
                functionpath[0] = false;
                functionpath[1] = false;
                functionpath[2] = true;
            }
           

           
           
                
            
            
            
        }

        private IEnumerator attackf()
        {
            yield return new WaitForSeconds(2);
            if (time1 >= 4)
            {
               
                x = Instantiate(punch, punchspot.transform.position, transform.rotation);
                    
                
                punchrb = x.GetComponent<Rigidbody>();
                punchrb.AddForce(punchspot.transform.forward*punch_speed, ForceMode.Impulse);
                
                 
                
               
                
            }
           
            yield return new WaitForSeconds(2);
            if (Player_Detected == false && time1 < 3)
            {
                functionpath[0] = true;
                functionpath[2] = false;
            }
            time1 = 0;
            
        }
        void attackfunction()
        {
            if (time1 >= 4)
            {
               
                    x = Instantiate(punch, punchspot.transform.position, punch.transform.rotation);
                    
                
                 punchrb = x.GetComponent<Rigidbody>();
                 punchrb.AddForce(punchspot.transform.forward*punch_speed, ForceMode.Impulse);
                
                 
                
               
                
            }
            time1 = 0;
           
            if (Player_Detected == false && time1 < 3)
            {
                functionpath[0] = true;
                functionpath[2] = false;
            }
        }
        

        private void OnTriggerEnter(Collider other)
        {
            foreach (Transform child in other.transform)
            {
                if (child.gameObject.CompareTag("Player"))
                {
                    player = child.gameObject;
                    player = child.gameObject;
                    Player_Detected = true;
                    functionpath[0] = false;
                    functionpath[1] = true;
                }
                
            }
            if (other.gameObject.CompareTag("Player"))
            {
                ActualPlayer = other.gameObject;
            }
        }
        private void OnTriggerStay(Collider other)
        {
            foreach (Transform child in other.transform)
            {
                if (child.gameObject.CompareTag("Player"))
                {
                    player = child.gameObject;
                   
                    
                
               
                }
                
                
            }
            
        }
    }
}
