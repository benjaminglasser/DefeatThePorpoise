using UnityEngine;

public class CreateAsteroidField : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public int numberOfObjects = 10;
    public Vector3 fieldSize = new Vector3(10f, 10f, 10f);
    public Vector2 minMaxScale = new Vector2(0.5f, 2f);

    void Start()
    {

        // Safety limit
        int count = Mathf.Clamp(numberOfObjects, 0, 5000);

        for (int i = 0; i < count; i++)
        {
            GameObject asteroid = Instantiate(asteroidPrefab, transform);

            asteroid.transform.localPosition = new Vector3(
                Random.Range(-fieldSize.x / 2f, fieldSize.x / 2f),
                Random.Range(-fieldSize.y / 2f, fieldSize.y / 2f),
                Random.Range(-fieldSize.z / 2f, fieldSize.z / 2f)
            );

            asteroid.transform.localRotation = Random.rotation;

            float randomScale = Random.Range(minMaxScale.x, minMaxScale.y);

            asteroid.transform.localScale =
                Vector3.one * randomScale;
        }
    }
}