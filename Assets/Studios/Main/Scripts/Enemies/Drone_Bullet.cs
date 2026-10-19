using UnityEngine;

public class Drone_Bullet : MonoBehaviour
{
    public float lifeTime = 5f;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // ignore the drone that shot it
        if (other.GetComponentInParent<Drone_Script>() != null) return;

        if (other.CompareTag("Player"))
        {
            // swap this out once the health script exists
            Debug.Log("Drone bullet hit the player for " + damage);
        }

        Destroy(gameObject);
    }
}

