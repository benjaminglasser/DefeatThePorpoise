using UnityEngine;

public class orbMovement : MonoBehaviour
{
    [Header("Linear Movement")]
    [Tooltip("Minimum asteroid drift speed")]
    public float minMoveSpeed = 0.5f;

    [Tooltip("Maximum asteroid drift speed")]
    public float maxMoveSpeed = 2.5f;

    [Header("Rotation")]
    [Tooltip("Minimum tumble speed")]
    public float minRotationSpeed = 10f;

    [Tooltip("Maximum tumble speed")]
    public float maxRotationSpeed = 40f;

    [Header("Direction Wandering")]
    [Tooltip("How much the asteroid's travel direction slowly changes")]
    public float wanderStrength = 0.25f;

    [Tooltip("How quickly its movement direction changes")]
    public float wanderSpeed = 0.5f;

    private Vector3 moveDirection;
    private Vector3 rotationAxis;

    private float moveSpeed;
    private float rotationSpeed;

    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;

    private void Start()
    {
        // Random movement direction through 3D space
        moveDirection = Random.onUnitSphere.normalized;

        // Random movement speed
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

        // Random tumble axis
        rotationAxis = Random.onUnitSphere.normalized;

        // Random tumble speed
        rotationSpeed = Random.Range(
            minRotationSpeed,
            maxRotationSpeed
        );

        // Different noise values for every asteroid
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        noiseOffsetZ = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        // Slowly changing random movement
        Vector3 wander = new Vector3(
            Mathf.PerlinNoise(
                noiseOffsetX,
                Time.time * wanderSpeed
            ) - 0.5f,

            Mathf.PerlinNoise(
                noiseOffsetY,
                Time.time * wanderSpeed
            ) - 0.5f,

            Mathf.PerlinNoise(
                noiseOffsetZ,
                Time.time * wanderSpeed
            ) - 0.5f
        );

        moveDirection += wander * wanderStrength * Time.deltaTime;

        moveDirection.Normalize();

        // Move through space
        transform.position +=
            moveDirection *
            moveSpeed *
            Time.deltaTime;

        // Tumble
        transform.Rotate(
            rotationAxis,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}