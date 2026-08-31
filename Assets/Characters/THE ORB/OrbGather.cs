using UnityEngine;

public class OrbGather : MonoBehaviour
{
    // If player collides with orb, destroy the orb and add to score
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Destroy(gameObject);
                // Debug.Log("Hello!!!");
            }
        }
        
        

}
