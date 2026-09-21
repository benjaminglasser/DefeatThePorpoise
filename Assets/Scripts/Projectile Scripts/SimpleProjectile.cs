using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float lifetime = 5f;

    [Header("Hit Detection")]
    public float hitRadius = 0.6f;

    [Tooltip("Set this to your asteroid layer")]
    public LayerMask asteroidLayer;

    private Vector3 direction;
    private float speed;
    private bool fired = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Fire(
        Vector3 shootDirection,
        float totalSpeed
    )
    {
        direction = shootDirection.normalized;
        speed = totalSpeed;

        transform.rotation =
            Quaternion.LookRotation(direction);

        fired = true;
    }

    private void LateUpdate()
    {
        if (!fired)
            return;

        Vector3 start =
            transform.position;

        Vector3 movement =
            direction *
            speed *
            Time.deltaTime;

        float distance =
            movement.magnitude;

        if (distance <= 0f)
            return;

        // Check EVERYTHING between current and next position.
        if (Physics.SphereCast(
            start,
            hitRadius,
            direction,
            out RaycastHit hit,
            distance,
            asteroidLayer,
            QueryTriggerInteraction.Collide
        ))
        {
            OrbGather orb =
                hit.collider.GetComponent<OrbGather>();

            if (orb == null)
            {
                orb =
                    hit.collider.GetComponentInParent<OrbGather>();
            }

            if (orb != null)
            {
                Debug.Log(
                    "Projectile popped: " +
                    orb.gameObject.name
                );

                orb.CollectOrb();

                Destroy(gameObject);

                return;
            }
        }

        transform.position =
            start + movement;
    }
}