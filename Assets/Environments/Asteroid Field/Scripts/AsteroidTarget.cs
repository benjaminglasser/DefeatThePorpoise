using UnityEngine;

public class AsteroidTarget : MonoBehaviour
{
    public GameObject popEffect;

    public void Pop()
    {
        if (popEffect != null)
        {
            Instantiate(
                popEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}