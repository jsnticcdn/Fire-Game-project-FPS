using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class Camera_Facer : MonoBehaviour
    {
        public GameObject Playere;
        public MaxCrabScript max_crabplayerdetect;
        public bool Lock_Y_Rotation = false;
        
        public bool forcrabscript;
        private bool isdetected;
        public bool forany;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            try
            {
                isdetected = max_crabplayerdetect.Player_Detected;
            }
            catch
            {
                Debug.Log(".");
            }
            
            if (isdetected)
            {
                Playere = max_crabplayerdetect.player;
            }
            if (forcrabscript)
            {
                if (isdetected)
                {
                    transform.LookAt(Playere.transform, Vector3.up);
                    if (Lock_Y_Rotation)
                    {
                        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    }
          
                    transform.Rotate(transform.up, 90 * Time.deltaTime);
                    if (transform.rotation.eulerAngles.x < -20)
                    {
                        transform.eulerAngles = new Vector3(-20, transform.eulerAngles.y, transform.eulerAngles.z);
                
                    }
                    
                }
                
               
                
            }
            else if (forany)
            {
                
                if (isdetected)
                {
                    transform.LookAt(Playere.transform, Vector3.up);
                    if (Lock_Y_Rotation)
                    {
                        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    }
          
                    transform.Rotate(transform.up, 90 * Time.deltaTime);
                    if (transform.rotation.eulerAngles.x < -20)
                    {
                        transform.eulerAngles = new Vector3(-20, transform.eulerAngles.y, transform.eulerAngles.z);
                
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
