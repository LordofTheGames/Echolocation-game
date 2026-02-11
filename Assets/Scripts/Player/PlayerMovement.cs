using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    private float minSpeed = 5f;
    private float maxSpeed = 20f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float scrollStep = 1.5f;


    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction scrollAction;
    public Vector3 MoveVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump"); 
        scrollAction = InputSystem.actions.FindAction("Scroll");
    }

    // Update is called once per frame
    void Update()
    {
        if (scrollAction != null){
            Vector2 scroll = scrollAction.ReadValue<Vector2>();
            if (Mathf.Abs(scroll.y) > 0.01f){
                speed += Mathf.Sign(scroll.y) * scrollStep;
                speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
            }
        }

        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveValue.x + transform.forward * moveValue.y;
        MoveVelocity = move * speed;

        controller.Move(speed * Time.deltaTime * move);

        if (jumpAction.IsPressed() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
