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
    private PlayerFootsteps footstepsScript;

    private float sprintTimer = 0f;
    private bool hasCompletedSprintTask = false;
    private float crouchTimer = 0f;
    private bool hasCompletedCrouchTask = false;
    private float walkTimer = 0f;
    private bool hasCompletedWalkTask = false;

    private CrazyTimer crazyTimer;


    // hold shift
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isCrouching)
            {
                isCrouching = false;
                isSprinting = true;
                footstepsScript.CurrentState = MoveState.SPRINT;
                return; 
            }

            isSprinting = !isSprinting;
            if (isSprinting) 
            {
                footstepsScript.CurrentState = MoveState.SPRINT;
            }
            else 
            {
                footstepsScript.CurrentState = MoveState.WALK;
            }
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = !isCrouching; 
            if (isCrouching) 
            {
                footstepsScript.CurrentState = MoveState.CROUCH;
                isSprinting = false;
            }
            else footstepsScript.CurrentState = MoveState.WALK;
        }
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
        footstepsScript = gameObject.GetComponent<PlayerFootsteps>();
        crazyTimer = gameObject.GetComponent<CrazyTimer>();
        defaultCamY = cameraTransform.localPosition.y;
    }
    // Update is called once per frame
    void Update()
    {

        // change height when crouching
        float targetYScale = isCrouching? 0.62f : 1;
        Vector3 scale = transform.localScale;
        scale.y =  Mathf.Lerp(transform.localScale.y, targetYScale, Time.deltaTime * 10);
        transform.localScale = scale;

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
        {
            currentSpeed = crouchSpeed;

            if (!moveInput.Equals(Vector2.zero) && !hasCompletedCrouchTask)
            {
                crouchTimer += Time.deltaTime;
                if (crouchTimer >= 3f)
                {
                    hasCompletedCrouchTask = true;
                    Debug.Log("Crouch Task Completed!");
                    TutorialManager.OnTaskComplete?.Invoke("Crouch"); 
                }
            }
        }
        else if (sprintAllowed && isSprinting)
        {
            currentSpeed = sprintSpeed;

            if(moveInput.Equals(Vector2.zero)){ 
                currentSpeed = walkSpeed;
                isSprinting = false;
                footstepsScript.CurrentState = MoveState.WALK;
            }
            // else if (!hasCompletedSprintTask)
            // {
            //     sprintTimer += Time.deltaTime;
            //     if (sprintTimer >= 5f)
            //     {
            //         hasCompletedSprintTask = true;
            //         Debug.Log("Completed!");
            //         TutorialManager.OnTaskComplete?.Invoke("Sprint"); 
            //     }
            // }
        }
        else 
        {
            currentSpeed = walkSpeed;
            if (!moveInput.Equals(Vector2.zero) && !hasCompletedWalkTask)
            {
                walkTimer += Time.deltaTime;
                if (walkTimer >= 8f)
                {
                    hasCompletedWalkTask = true;
                    Debug.Log("Walk Task Completed!");
                    TutorialManager.OnTaskComplete?.Invoke("Walk"); 
                }
            }
        }
        

        // play crazy effect faster if sprinting, turn it off if not
        crazyTimer.isSprinting = isSprinting;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (controller.enabled)
            controller.Move(currentSpeed * Time.deltaTime * move);


        velocity.y += gravity * Time.deltaTime;
        if (controller.enabled)
            controller.Move(velocity * Time.deltaTime);
    }

    public bool GetSprint()
    {
        return isSprinting;
    }

    public bool GetCrouch()
    {
        return isCrouching;
    }
}
