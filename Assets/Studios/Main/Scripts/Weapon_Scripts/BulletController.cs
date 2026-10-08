using System;
using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class BulletController : MonoBehaviour
    {
       public Collider bulletCollider;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Bullet")
            {
                bulletCollider.enabled = false;
                
            }
            else if (collision.gameObject.tag == "Wall")
            {
                bulletCollider.enabled = true;
                Destroy(this);
            }
           
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.tag == "Bullet")
            {
                bulletCollider.enabled = false;
                
            }
            else if (collision.gameObject.tag == "Wall")
            {
                bulletCollider.enabled = true;
                Destroy(this);
            }
            
        }
    }
}
