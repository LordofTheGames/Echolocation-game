using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float crouchSpeed = 3f;
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public bool jumpEnabled = false;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;
    Vector2 moveInput;  // Stores WASD input

    private bool isCrouching = false;
    private bool isSprinting = false;

    public Transform cameraTransform;
    private float defaultCamY;

    

    // hold shift
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            isSprinting = !isSprinting;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
            isCrouching = !isCrouching; 
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (jumpEnabled) Jump();
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void Start()
    {
        defaultCamY = cameraTransform.localPosition.y;
    }
    // Update is called once per frame
    void Update()
    {

        // change height when crouching
        float targetY = isCrouching ? defaultCamY - 0.3f : defaultCamY;

        Vector3 camPos = cameraTransform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * 10);
        cameraTransform.localPosition = camPos;

        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        // if crouching, sprinting is not allowed
        bool sprintAllowed = !isCrouching;

        float currentSpeed = walkSpeed;

        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (sprintAllowed && isSprinting)
        {
            currentSpeed = sprintSpeed;

            if(moveInput.Equals(Vector2.zero)){ 
                currentSpeed = walkSpeed;
                isSprinting = false;
            }
        }
        

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(currentSpeed * Time.deltaTime * move);


        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public bool GetSprint()
    {
        return isSprinting;
    }
}
