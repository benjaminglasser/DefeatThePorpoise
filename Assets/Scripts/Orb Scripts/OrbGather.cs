using UnityEngine;

public class OrbGather : MonoBehaviour
{
    public GameObject popEffect;

    private bool collected = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>();
    }

    public void CollectOrb()
    {
        if (collected)
            return;

        collected = true;

        if (popEffect != null)
        {
            Instantiate(
                popEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // Play sound from a temporary object
        if (audioSource != null && audioSource.clip != null)
        {
            GameObject tempAudio =
                new GameObject("Orb Pop Sound");

            tempAudio.transform.position =
                transform.position;

            AudioSource tempSource =
                tempAudio.AddComponent<AudioSource>();

            tempSource.clip =
                audioSource.clip;

            tempSource.volume =
                audioSource.volume;

            tempSource.pitch =
                audioSource.pitch;

            tempSource.spatialBlend =
                audioSource.spatialBlend;

            tempSource.Play();

            Destroy(
                tempAudio,
                audioSource.clip.length + 0.1f
            );
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player")
        )
        {
            CollectOrb();
        }
    }
}