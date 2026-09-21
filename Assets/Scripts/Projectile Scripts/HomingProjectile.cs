using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float lifetime = 5f;
    public float hitRadius = 0.4f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(
        Vector3 shooterVelocity,
        Vector3 shootDirection,
        float muzzleSpeed
    )
    {
        shootDirection.Normalize();

        transform.rotation =
            Quaternion.LookRotation(shootDirection);

        // Standard moving-vehicle projectile behavior:
        // inherit the dolphin velocity ONCE,
        // then add the projectile's muzzle velocity.
        rb.linearVelocity =
            shooterVelocity +
            shootDirection * muzzleSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        OrbGather orb =
            other.GetComponentInParent<OrbGather>();

        if (orb != null)
        {
            orb.CollectOrb();
            Destroy(gameObject);
        }
    }
}