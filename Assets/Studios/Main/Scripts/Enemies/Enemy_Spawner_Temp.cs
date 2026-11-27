using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class Enemy_Spawner_Temp : MonoBehaviour
    {
        public float time = 0;
        public float cooldown = 10;
        public float count = -8;
        public float maxenemies = 8;
        private Vector3 offset = new Vector3(1.5f, .5f, 1.5f);
        public GameObject enemy;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
           
        
        }

        void FixedUpdate()
        {
            if (time >= cooldown)
            {
                time = 0;
                if (count < maxenemies)
                {
                    
                    Instantiate(enemy, transform.position + offset, transform.rotation);
                    time = 0;
                    count++;
                    
                }
                
                
            }
            
        }
    }
}
