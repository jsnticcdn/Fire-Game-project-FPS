using UnityEngine;
using UnityEngine.InputSystem;


namespace AZE.AdvancedFirstPerson
{
    public class Reloading_Script : MonoBehaviour
    {

        public int ClickCount;
        public int maxAmmo = 6;
        public int currentAmmo;
        public float ReloadTime;
         
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            currentAmmo = maxAmmo - ClickCount;
            if (currentAmmo <= 0) ;

        }

        

        // Update is called once per frame
        void Update()
        {
           ReloadTime += Time.deltaTime;
            
            
            if (Input.GetMouseButtonDown(0))
                ClickCount++;
        }
    }
}
    
