using System;
using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class bullet_controller : MonoBehaviour
    {
        public float time = 0;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            

        }

        private void FixedUpdate()
        {
            time += Time.deltaTime;
            if (time > 3)
            {
                time = 0;
                Destroy(gameObject);
            }
        }
    }
}
