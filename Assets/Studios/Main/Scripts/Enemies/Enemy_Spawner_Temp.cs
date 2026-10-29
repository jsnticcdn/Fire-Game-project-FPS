using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class Enemy_Spawner_Temp : MonoBehaviour
    {
        private float time = 0;

        private float maxenemies = 8;
        private Vector3 offset = new Vector3(1, .5f, 1);
        public GameObject enemy;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            if (time > 5)
            {
                if (maxenemies > 0)
                {
                    
                    Instantiate(enemy, transform.position + offset, transform.rotation);
                    time = 0;
                    
                }
                
            }
        
        }
    }
}
