using System.Collections;
using UnityEngine;

public class DolphinShooter : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform projectileSpawnPoint;
    public LineRenderer laserLine;

    [Header("Targeting")]
    public LayerMask asteroidLayer;

    [Tooltip("Maximum distance the laser can travel")]
    public float maxShootDistance = 1500f;

    [Header("Laser Visual")]
    [Tooltip("How long the laser stays visible")]
    [Range(0.01f, 0.5f)]
    public float laserDuration = 0.08f;

    private Coroutine laserCoroutine;

    private bool laserActive = false;
    private Vector3 laserEndPoint;


    private void Awake()
    {
        if (laserLine != null)
        {
            laserLine.positionCount = 2;
            laserLine.useWorldSpace = true;
            laserLine.enabled = false;
        }
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }


    private void LateUpdate()
    {
        // Keep the beginning of the beam attached
        // to the dolphin's nose while it is visible.
        if (
            laserActive &&
            laserLine != null &&
            projectileSpawnPoint != null
        )
        {
            laserLine.SetPosition(
                0,
                projectileSpawnPoint.position
            );

            laserLine.SetPosition(
                1,
                laserEndPoint
            );
        }
    }


    private void Shoot()
    {
        if (
            playerCamera == null ||
            projectileSpawnPoint == null ||
            laserLine == null
        )
        {
            return;
        }

        // Ray directly through the mouse cursor
        Ray mouseRay =
            playerCamera.ScreenPointToRay(
                Input.mousePosition
            );


        // Look specifically for something on the asteroid layer
        if (Physics.Raycast(
            mouseRay,
            out RaycastHit hit,
            maxShootDistance,
            asteroidLayer,
            QueryTriggerInteraction.Collide
        ))
        {
            // Stop the laser exactly where it hit
            laserEndPoint = hit.point;


            // Find OrbGather on the hit object
            // OR one of its parents
            OrbGather orb =
                hit.collider.GetComponentInParent<OrbGather>();


            if (orb != null)
            {
                orb.CollectOrb();
            }
        }
        else
        {
            // No asteroid hit.
            // Laser still shoots toward the cursor.
            laserEndPoint =
                mouseRay.GetPoint(maxShootDistance);
        }


        ShowLaser();
    }


    private void ShowLaser()
    {
        laserActive = true;

        laserLine.enabled = true;


        laserLine.SetPosition(
            0,
            projectileSpawnPoint.position
        );

        laserLine.SetPosition(
            1,
            laserEndPoint
        );


        if (laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
        }


        laserCoroutine =
            StartCoroutine(
                HideLaserAfterDelay()
            );
    }


    private IEnumerator HideLaserAfterDelay()
    {
        yield return new WaitForSeconds(
            laserDuration
        );


        laserActive = false;

        if (laserLine != null)
        {
            laserLine.enabled = false;
        }

        laserCoroutine = null;
    }
}