using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AZE.AdvancedFirstPerson
{
    public class Idle_Weapon_Script : MonoBehaviour
    {
        public InputAction take_gun;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }



        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                transform.position = new Vector3(100,100,100);


            }
        }
    }
}
