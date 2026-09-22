using UnityEngine;

public class AltCharacterController : MonoBehaviour
{
    [Header("Movement")]
    public float throttleIncrement = 5f;
    public float maxThrust = 200f;
    public float responsiveness = 10f;

    [Tooltip("Minimum throttle so the dolphin is always moving forward")]
    public float minimumThrottle = 2f;

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

    [Tooltip("Velocity where ribbon starts becoming brighter")]
    public float trailMinBrightnessSpeed = 5f;

    [Tooltip("Velocity where ribbon reaches full brightness")]
    public float trailFullBrightnessSpeed = 150f;


    private float throttle;

    private float roll;
    private float pitch;
    private float yaw;

    private float targetRoll;
    private float targetPitch;
    private float targetYaw;

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
            dolphinSound.volume = throttle * 0.01f;
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
        rb.AddForce(
            maxThrust * throttle * transform.forward
        );
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
        if (trailMaterial == null || rb == null)
            return;

        float speed = rb.linearVelocity.magnitude;

        float speed01 = Mathf.InverseLerp(
            trailMinBrightnessSpeed,
            trailFullBrightnessSpeed,
            speed
        );

        trailMaterial.SetFloat(
            "_speed01",
            speed01
        );
    }
}