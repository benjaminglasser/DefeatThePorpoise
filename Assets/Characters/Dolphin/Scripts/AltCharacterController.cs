using UnityEngine;

public class AltCharacterController : MonoBehaviour
{
    [Header("Movement")]
    public float throttleIncrement = 5f;
    public float maxThrust = 200f;
    public float responsiveness = 10f;

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


    private float responseModifier
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

        if (dolphinSound != null)
        {
            dolphinSound.volume = throttle * 0.01f;
        }
    }


    private void HandleInputs()
    {
        // Desired rotation input
        targetRoll = Input.GetAxis("Roll");
        targetPitch = Input.GetAxis("Pitch");
        targetYaw = Input.GetAxis("Yaw");

        // Hold Space to increase thrust
        if (Input.GetKey(KeyCode.Space))
        {
            throttle += throttleIncrement * Time.deltaTime;
        }
        else
        {
            throttle -= throttleIncrement * Time.deltaTime;
        }

        throttle = Mathf.Clamp(throttle, 0f, 100f);
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


    private void FixedUpdate()
    {
        // Forward thrust
        rb.AddForce(
            maxThrust * throttle * transform.forward
        );

        // Physical rotation
        Vector3 torque = new Vector3(
            pitch,
            yaw,
            -roll
        );

        rb.AddRelativeTorque(
            torque * responseModifier
        );
    }


    private void UpdateVisualMovement()
    {
        if (visualModel == null)
            return;

        // Roll input banks the dolphin.
        // Yaw input ALSO banks the dolphin into the turn.
        float bankInput =
            (-roll * rollBankInfluence) +
            (-yaw * yawBankInfluence);

        // Clamp so combined yaw + roll can't produce absurd banking.
        bankInput = Mathf.Clamp(
            bankInput,
            -1f,
            1f
        );

        float bank =
            bankInput * visualBankAngle;

        // Slight visual exaggeration of pitch
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
}