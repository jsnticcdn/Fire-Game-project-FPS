using System;
using UnityEngine;
using System.Collections;
namespace AZE.AdvancedFirstPerson
{
    public class BulletController : MonoBehaviour
    {
       public GameObject bullet;
       
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
           Destroy(this.gameObject, 5f);
        }

        // Update is called once per frame
        void Update()
        {
           // StartCoroutine(DestroyBullet());

        }

        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Wall")
            {
                Destroy(gameObject);
            }
            
           
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.tag == "Wall")
            {
                Destroy(gameObject);
            }

            
            
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.tag == "Wall")
            {
                Destroy(gameObject);
            }
        }
        
    }
}
