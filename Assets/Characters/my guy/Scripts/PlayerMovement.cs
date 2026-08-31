using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector2 movementInput;
    private float verticalInput;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMovement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    public void OnVertical(InputValue value)
    {
        verticalInput = value.Get<float>();
    }

    private void FixedUpdate()
    {

        Vector2 horizontalInput = Vector2.ClampMagnitude(movementInput, 1f);

        // Create a direction from WASD and the vertical controls.
        Vector3 movementDirection = new Vector3(horizontalInput.x, verticalInput, horizontalInput.y);

        

        Vector3 velocity = new Vector3(
            movementDirection.x * speed,
            movementDirection.y * verticalSpeed,
            movementDirection.z * speed
        );

        rb.linearVelocity = velocity;

        // Rotate only toward horizontal movement.
        Vector3 horizontalDirection = new Vector3(movementDirection.x, 0f, movementDirection.z);

        if (horizontalDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                horizontalDirection,
                Vector3.up
            );

            Quaternion smoothRotation = Quaternion.Slerp(rb.rotation,targetRotation,rotationSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(smoothRotation);
        }
    }
}