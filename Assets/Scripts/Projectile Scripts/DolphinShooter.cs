using UnityEngine;

public class DolphinShooter : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public Rigidbody dolphinRigidbody;

    [Header("Shooting")]
    [Tooltip("Base projectile speed")]
    public float projectileSpeed = 4000f;

    [Tooltip("How far along the mouse ray to aim")]
    public float aimDistance = 4000f;

    private void Awake()
    {
        if (dolphinRigidbody == null)
        {
            dolphinRigidbody = GetComponentInParent<Rigidbody>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (
            playerCamera == null ||
            projectileSpawnPoint == null ||
            projectilePrefab == null
        )
        {
            return;
        }

        // Ray directly through mouse cursor
        Ray cursorRay =
            playerCamera.ScreenPointToRay(Input.mousePosition);

        // Pick a distant point along that ray
        Vector3 aimPoint =
            cursorRay.GetPoint(aimDistance);

        // Projectile begins exactly at the dolphin's muzzle
        Vector3 spawnPosition =
            projectileSpawnPoint.position;

        // Direction from muzzle toward the cursor
        Vector3 shootDirection =
            (aimPoint - spawnPosition).normalized;

        // Create the projectile
        GameObject projectile =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(shootDirection)
            );

        SimpleProjectile bullet =
            projectile.GetComponent<SimpleProjectile>();

        if (bullet == null)
        {
            Debug.LogError(
                "Projectile prefab is missing SimpleProjectile."
            );

            return;
        }

        // Add dolphin's current speed so the bolt
        // always moves faster than the player.
        float dolphinSpeed = 0f;

        if (dolphinRigidbody != null)
        {
            dolphinSpeed =
                dolphinRigidbody.linearVelocity.magnitude;
        }

        float totalProjectileSpeed =
            projectileSpeed + dolphinSpeed;

        // Fire the projectile and give it the muzzle
        // so its Line Renderer can stay attached.
        bullet.Fire(
            shootDirection,
            totalProjectileSpeed,
            projectileSpawnPoint
        );
    }
}