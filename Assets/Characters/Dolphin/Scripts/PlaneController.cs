using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header ("Plane Stats")]
    [Tooltip ("How much the throttle ramps up or down")]
    public float throttleIncrement = 5f;
    [Tooltip ("Maximum engine thrust when at 100% throttle")]
    public float maxThrust = 200f;
    [Tooltip ("How responsive the plane is when rolling, pitching, or yawing")]
    public float responsiveness = 10f;

    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;

    private float responseModifier
    {
        get {return (rb.mass / 10f) * responsiveness; }
    }
    

    Rigidbody rb;
    AudioSource dolphinSound;

    private void Awake() 
    {
    rb = GetComponent<Rigidbody>();
    dolphinSound = GetComponent<AudioSource>();
    }
    private void HandleInputs()
    {
        // Set rotational values from our axis inputs.
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        // Handle throttle value being sure to clamp it between 0 and 100.
        if (Input.GetKeyDown(KeyCode.Space))
        {
        //add throttleIncrement to the throttle value over time while the space key is held down
        throttle += throttleIncrement * Time.deltaTime;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            throttle -= throttleIncrement * Time.deltaTime;
        }
        throttle = Mathf.Clamp(throttle, 0f, 100f);
        }

        private void Update()
        {
            HandleInputs();
            // Set the volume of the dolphin sound based on the throttle value.
            dolphinSound.volume = throttle * 0.01f;
        }

        private void FixedUpdate()
        {
            // Apply the thrust force to the plane.
            rb.AddForce(maxThrust * throttle * transform.forward);

            // Apply torque to the plane based on our input values and responsiveness.
            rb.AddRelativeTorque(new Vector3(pitch, yaw, -roll) * responseModifier);
        }
    
}
