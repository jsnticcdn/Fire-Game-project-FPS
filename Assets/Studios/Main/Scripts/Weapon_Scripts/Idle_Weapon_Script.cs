using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AZE.AdvancedFirstPerson
{
    public class Idle_Weapon_Script : MonoBehaviour
    {
        public InputAction take_gun;
        public GameObject player;
        public float istakingweapon = 0;
    
        public bool isparent = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            take_gun.Enable();

        }

        // Update is called once per frame
        void Update()
        {
            
            istakingweapon = take_gun.ReadValue<float>();
            if (isparent)
            {
                transform.position = player.transform.position + new Vector3(0, 0, 0);
            }

        }



        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (istakingweapon > 0)
                {
                    isparent = true;
                    transform.SetParent(player.transform);
                    
                }
                


            }
        }
    }
}
