using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 movementInput;
    public Vector3 movementDirection;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 10f;


    public void OnMovement(InputValue value)
        {
            movementInput = value.Get<Vector2>();
        }


    private void Update()
    {
    //  use WASD to add Vector2 values to the X and Z component of transofrm on capsule character
        movementDirection = new Vector3(movementInput.x, 0, movementInput.y);
        movementDirection.Normalize();
        
        transform.Translate(movementDirection * Time.deltaTime * speed, Space.World);

        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

}
