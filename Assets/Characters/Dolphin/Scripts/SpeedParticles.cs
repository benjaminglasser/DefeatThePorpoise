using UnityEngine;
using UnityEngine.VFX;

public class SpeedParticles : MonoBehaviour
{
    [Header("References")]
    public Rigidbody dolphin;
    public VisualEffect speedVFX;

    [Header("Speed")]
    public float maxSpeed = 70f;

    private void Awake()
    {
        if (dolphin == null)
        {
            dolphin = GetComponentInParent<Rigidbody>();
        }

        if (speedVFX == null)
        {
            speedVFX = GetComponent<VisualEffect>();
        }
    }

    private void Update()
    {
        if (dolphin == null || speedVFX == null)
            return;

        float currentSpeed = dolphin.linearVelocity.magnitude;

        float speed01 = Mathf.Clamp01(
            currentSpeed / maxSpeed
        );

        // Controls both VFX systems
        speedVFX.SetFloat("speed01", speed01);

        // Gives the ribbon the current tail position in world space
        speedVFX.SetVector3(
            "TrailPosition",
            transform.position
        );
    }
}