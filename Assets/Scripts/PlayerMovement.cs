using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 movementInput;
    public Vector3 movementDirection;

    [SerializeField] private float speed = 10f;


    public void OnMovement(InputValue value)
        {
            movementInput = value.Get<Vector2>();
        }


    private void Update()
    {
    //  use WASD to add Vector2 values to the X and Z component of transofrm on capsule character
        movementDirection = new Vector3(movementInput.x, 0, movementInput.y);
        transform.Translate(movementDirection * Time.deltaTime * speed);
        // Debug.Log(movementDirection);
        // Debug.Log(movementInput);

    }

}
