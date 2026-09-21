using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float lifetime = 5f;

    [Header("Hit Detection")]
    public float hitRadius = 0.6f;
    public LayerMask asteroidLayer;

    [Header("Laser Visual")]
    public LineRenderer laserLine;

    [Tooltip("How long the laser stays visible")]
    [Range(0.01f, 0.5f)]
    public float laserDuration = 0.08f;

    [Tooltip("How quickly the visible laser tip catches up to the real projectile")]
    public float laserVisualSpeed = 1200f;

    private Vector3 direction;
    private float speed;

    private bool fired = false;

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

        if (muzzlePoint != null)
        {
            visualLaserEnd = muzzlePoint.position;
        }
        else
        {
            visualLaserEnd = transform.position;
        }

        laserTimer = laserDuration;

        if (laserLine != null)
        {
            laserLine.enabled = true;
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

        // Smoothly move the visible tip toward the
        // actual high-speed projectile.
        visualLaserEnd =
            Vector3.MoveTowards(
                visualLaserEnd,
                transform.position,
                laserVisualSpeed * Time.deltaTime
            );

        // Start stays locked to dolphin nose
        laserLine.SetPosition(
            0,
            muzzlePoint.position
        );

        // End moves smoothly instead of jumping 60+ units/frame
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