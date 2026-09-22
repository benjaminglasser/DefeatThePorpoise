using UnityEngine;

public class AltCharacterController : MonoBehaviour
{
    [Header("Movement")]
    public float throttleIncrement = 20f;
    public float maxThrust = 16f;
    public float responsiveness = 20f;
    public float minimumThrottle = 1.5f;

    [Tooltip("Maximum physical movement speed")]
    public float maxSpeed = 15f;

    [Header("Braking")]
    [Tooltip("How aggressively the dolphin slows down when thrust is released")]
    public float brakeStrength = 5f;

    [Header("Input Smoothing")]
    [Tooltip("Higher = controls react faster")]
    public float inputSmoothness = 5f;

    [Header("Rotation Feel")]
    [Tooltip("Helps stop endless spinning")]
    public float angularDamping = 2f;

    [Tooltip("Limits how fast the Rigidbody can rotate")]
    public float maxAngularVelocity = 3f;

    [Header("Visual Model")]
    [Tooltip("Drag the dolphin visual/model child here")]
    public Transform visualModel;

    [Tooltip("Maximum visual bank angle")]
    public float visualBankAngle = 35f;

    [Tooltip("Extra visual pitch when pitching")]
    public float visualPitchAngle = 12f;

    [Tooltip("How quickly the dolphin model eases into turns")]
    public float visualTurnSmoothness = 5f;

    [Header("Natural Banking")]
    [Tooltip("How much roll input contributes to visual banking")]
    public float rollBankInfluence = 0.7f;

    [Tooltip("How much yaw input contributes to visual banking")]
    public float yawBankInfluence = 1f;

    [Header("Ribbon Trail")]
    [Tooltip("Material used by the ribbon trail")]
    public Material trailMaterial;

    [Tooltip("Throttle where ribbon begins brightening")]
    public float trailMinBrightnessThrottle = 1.5f;

    [Tooltip("Throttle where ribbon reaches its target maximum")]
    public float trailFullBrightnessThrottle = 100f;

    [Tooltip("How quickly the ribbon brightens")]
    public float ribbonBrightenSpeed = 0.25f;

    [Tooltip("How quickly the ribbon fades")]
    public float ribbonFadeSpeed = 1f;

    [Range(0f, 1f)]
    [Tooltip("Maximum value sent to the shader's _speed01")]
    public float ribbonMaxIntensity = 0.7f;


    private float throttle;

    private float roll;
    private float pitch;
    private float yaw;

    private float targetRoll;
    private float targetPitch;
    private float targetYaw;

    private float ribbonIntensity;

    private Rigidbody rb;
    private AudioSource dolphinSound;

    private Quaternion visualStartingRotation;


    private float ResponseModifier
    {
        get
        {
            return (rb.mass / 10f) * responsiveness;
        }
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dolphinSound = GetComponent<AudioSource>();

        rb.angularDamping = angularDamping;
        rb.maxAngularVelocity = maxAngularVelocity;

        throttle = minimumThrottle;
        ribbonIntensity = 0f;

        if (visualModel != null)
        {
            visualStartingRotation = visualModel.localRotation;
        }
    }


    private void Update()
    {
        HandleInputs();
        SmoothInputs();
        UpdateVisualMovement();
        UpdateRibbonBrightness();

        if (dolphinSound != null)
        {
            dolphinSound.volume = Mathf.InverseLerp(
                minimumThrottle,
                100f,
                throttle
            );
        }
    }


    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }


    private void HandleInputs()
    {
        targetRoll = Input.GetAxis("Roll");
        targetPitch = Input.GetAxis("Pitch");
        targetYaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.Space))
        {
            throttle += throttleIncrement * Time.deltaTime;
        }
        else
        {
            throttle -= throttleIncrement * Time.deltaTime;
        }

        throttle = Mathf.Clamp(
            throttle,
            minimumThrottle,
            100f
        );
    }


    private void SmoothInputs()
    {
        roll = Mathf.Lerp(
            roll,
            targetRoll,
            inputSmoothness * Time.deltaTime
        );

        pitch = Mathf.Lerp(
            pitch,
            targetPitch,
            inputSmoothness * Time.deltaTime
        );

        yaw = Mathf.Lerp(
            yaw,
            targetYaw,
            inputSmoothness * Time.deltaTime
        );
    }


    private void ApplyMovement()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            // Accelerating
            rb.AddForce(
                maxThrust * throttle * transform.forward
            );
        }
        else
        {
            // Gentle idle thrust so the dolphin never fully stops
            rb.AddForce(
                maxThrust * minimumThrottle * transform.forward
            );

            // Active braking
            rb.AddForce(
                -rb.linearVelocity * brakeStrength,
                ForceMode.Acceleration
            );
        }

        // Hard speed cap
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }
    }


    private void ApplyRotation()
    {
        Vector3 torque = new Vector3(
            pitch,
            yaw,
            -roll
        );

        rb.AddRelativeTorque(
            torque * ResponseModifier
        );
    }


    private void UpdateVisualMovement()
    {
        if (visualModel == null)
            return;

        float bankInput =
            (-roll * rollBankInfluence) +
            (-yaw * yawBankInfluence);

        bankInput = Mathf.Clamp(
            bankInput,
            -1f,
            1f
        );

        float bank =
            bankInput * visualBankAngle;

        float nosePitch =
            pitch * visualPitchAngle;

        Quaternion targetRotation =
            visualStartingRotation *
            Quaternion.Euler(
                nosePitch,
                0f,
                bank
            );

        visualModel.localRotation =
            Quaternion.Slerp(
                visualModel.localRotation,
                targetRotation,
                visualTurnSmoothness * Time.deltaTime
            );
    }


    private void UpdateRibbonBrightness()
    {
        if (trailMaterial == null)
            return;

        // Desired ribbon intensity based on current throttle
        float targetIntensity = Mathf.InverseLerp(
            trailMinBrightnessThrottle,
            trailFullBrightnessThrottle,
            throttle
        );

        // Cap the overall maximum
        targetIntensity *= ribbonMaxIntensity;

        // Brighten slowly, fade faster
        float changeSpeed =
            targetIntensity > ribbonIntensity
            ? ribbonBrightenSpeed
            : ribbonFadeSpeed;

        ribbonIntensity = Mathf.MoveTowards(
            ribbonIntensity,
            targetIntensity,
            changeSpeed * Time.deltaTime
        );

        trailMaterial.SetFloat(
            "_speed01",
            ribbonIntensity
        );
    }
}