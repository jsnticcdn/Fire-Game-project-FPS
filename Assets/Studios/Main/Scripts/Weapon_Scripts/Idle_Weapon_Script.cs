using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AZE.AdvancedFirstPerson
{
    public class Idle_Weapon_Script : MonoBehaviour
    {
        public InputAction take_gun;
        public CinemachineCamera camera;
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
                //transform.position = player.transform.position + new Vector3(-0.5f, 1.5f, -0.5f);
                transform.rotation = player.transform.rotation;
                transform.position = camera.transform.position + new Vector3(1,0,1);
                transform.parent = player.transform;
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
