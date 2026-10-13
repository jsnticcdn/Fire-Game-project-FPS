using System;
using UnityEngine;
using System.Collections;
namespace AZE.AdvancedFirstPerson
{
    public class BulletController : MonoBehaviour
    {
       
       
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
           // StartCoroutine(DestroyBullet());

        }

        
        private void OnCollisionEnter(Collision collision)
        {
            
           
        }

        private void OnCollisionStay(Collision collision)
        {

            
            
        }

        private void OnCollisionExit(Collision collision)
        {
            //Destroy(gameObject);
        }
        
    }
}
