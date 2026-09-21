using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float lifetime = 2f;

    [Header("Hit Detection")]
    public float hitRadius = 0.6f;
    public LayerMask asteroidLayer;

    [Header("Laser Visual")]
    public LineRenderer laserLine;

    [Range(0.01f, 0.5f)]
    public float laserDuration = 0.08f;

    [Tooltip("Higher = laser endpoint follows projectile more tightly")]
    public float laserFollowSmoothness = 20f;

    private Vector3 direction;
    private float speed;
    private bool fired;

    private Transform muzzlePoint;

    private Vector3 visualLaserEnd;
    private float laserTimer;

    private void Awake()
    {
        if (laserLine == null)
        {
            laserLine = GetComponentInChildren<LineRenderer>();
        }

        if (laserLine != null)
        {
            laserLine.positionCount = 2;
            laserLine.enabled = false;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Fire(
        Vector3 shootDirection,
        float totalSpeed,
        Transform shooterMuzzle
    )
    {
        direction = shootDirection.normalized;
        speed = totalSpeed;

        muzzlePoint = shooterMuzzle;

        transform.rotation =
            Quaternion.LookRotation(direction);

        fired = true;

        laserTimer = laserDuration;

        if (muzzlePoint != null)
        {
            visualLaserEnd = muzzlePoint.position;
        }
        else
        {
            visualLaserEnd = transform.position;
        }

        if (laserLine != null)
        {
            laserLine.enabled = true;

            laserLine.SetPosition(
                0,
                muzzlePoint.position
            );

            laserLine.SetPosition(
                1,
                visualLaserEnd
            );
        }
    }

    private void Update()
    {
        if (!fired)
            return;

        MoveProjectile();

        laserTimer -= Time.deltaTime;

        if (laserTimer <= 0f && laserLine != null)
        {
            laserLine.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (
            !fired ||
            laserLine == null ||
            !laserLine.enabled ||
            muzzlePoint == null
        )
        {
            return;
        }

        // Smoothly follow the REAL projectile position.
        float smoothAmount =
            1f - Mathf.Exp(
                -laserFollowSmoothness * Time.deltaTime
            );

        visualLaserEnd =
            Vector3.Lerp(
                visualLaserEnd,
                transform.position,
                smoothAmount
            );

        // Muzzle end
        laserLine.SetPosition(
            0,
            muzzlePoint.position
        );

        // Smoothed visual end
        laserLine.SetPosition(
            1,
            visualLaserEnd
        );
    }

    private void MoveProjectile()
    {
        Vector3 movement =
            direction *
            speed *
            Time.deltaTime;

        float distance =
            movement.magnitude;

        if (distance > 0f)
        {
            if (Physics.SphereCast(
                transform.position,
                hitRadius,
                direction,
                out RaycastHit hit,
                distance,
                asteroidLayer,
                QueryTriggerInteraction.Collide
            ))
            {
                OrbGather orb =
                    hit.collider.GetComponentInParent<OrbGather>();

                if (orb != null)
                {
                    orb.CollectOrb();
                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position += movement;
    }
}