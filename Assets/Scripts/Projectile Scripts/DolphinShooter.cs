using UnityEngine;

public class DolphinShooter : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;

    [Header("Shooting")]
    public float projectileSpeed = 150f;
    public float aimDistance = 1000f;

    private Rigidbody dolphinRigidbody;

    private void Awake()
    {
        dolphinRigidbody = GetComponentInParent<Rigidbody>();
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
        if (playerCamera == null ||
            projectileSpawnPoint == null ||
            projectilePrefab == null)
        {
            return;
        }

        // Ray through the mouse cursor
        Ray mouseRay =
            playerCamera.ScreenPointToRay(Input.mousePosition);

        // Distant point along mouse ray
        Vector3 aimPoint =
            mouseRay.GetPoint(aimDistance);

        // Direction from dolphin muzzle toward mouse
        Vector3 shootDirection =
            (aimPoint - projectileSpawnPoint.position).normalized;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.LookRotation(shootDirection)
            );

        SimpleProjectile bullet =
            projectile.GetComponent<SimpleProjectile>();

        if (bullet == null)
        {
            Debug.LogError(
                "Projectile prefab needs SimpleProjectile."
            );

            return;
        }

        float dolphinSpeed = 0f;

        if (dolphinRigidbody != null)
        {
            dolphinSpeed =
                dolphinRigidbody.linearVelocity.magnitude;
        }

        bullet.Fire(
            shootDirection,
            projectileSpeed + dolphinSpeed
        );
    }
}