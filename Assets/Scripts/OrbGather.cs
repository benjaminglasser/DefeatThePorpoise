using UnityEngine;

public class OrbGather : MonoBehaviour

//if player collides with object, play sound and then destroy the object
{
    AudioSource collectSound;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            collectSound = GetComponent<AudioSource>();
            collectSound.Play();
            Destroy(gameObject, collectSound.clip.length);
        }
    }
}