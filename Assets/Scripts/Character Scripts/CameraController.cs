using UnityEngine;

public class CameraController : MonoBehaviour
{   
    [Tooltip ("An array of transforms representing camera positions.")]
    [SerializeField] Transform[] povs;
    [Tooltip ("The speed at which the camera follows the plane.")]
    [SerializeField] float speed;

    private int index = 0;
    private Vector3 target;
    
    private void Update()
    {
        // Change the camera position when the user presses a number key.
        if (Input.GetKeyDown(KeyCode.Alpha1))index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2))index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3))index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4))index = 3;

        // Set the target position to the relevant POV
        target = povs[index].position;
    }

    private void FixedUpdate()
    {
        // Move the camera towards the target position at a set speed.
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = povs[index].forward;
    }
}
