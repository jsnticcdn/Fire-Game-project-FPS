using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class EnemiesHit : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "TestEnemy" )
            {
                Destroy(collision.gameObject); 

            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
