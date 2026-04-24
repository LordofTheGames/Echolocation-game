using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 20f;
    public float gamepadSensitivity = 13f;
    public bool LimitVerticalLook = false;
    public float verticalLookLimit = 30f;
    public Transform playerBody;
    public Transform lightTransform;
    private float xRotation = 0f;
    private float yRotation = 0f; 
    private Vector2 lookInput; // Stores the current mouse delta
    public bool isHiding = false;
    public bool hidingTransition = false;

    private bool isGamepad; // Tracks which device is currently being used

    public void Start()
    {
        // we do this so the values in the inspector have a similar scale/size
        gamepadSensitivity *= 10;
        mouseSensitivity /= 100;
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        if (context.control != null)
            isGamepad = context.control.device is Gamepad;
    }

    public void SyncLookAngles(float pitch, float yaw)
    {
        xRotation = pitch;
        yRotation = yaw;
    }

    // Update is called once per frame
    void Update()
    {
        if (hidingTransition) return;

        float mouseX, mouseY;
        if (isGamepad)
        {
            // Gamepads output a continuous -1 to 1 value, so we must multiply by Time.deltaTime
            mouseX = lookInput.x * gamepadSensitivity * Time.deltaTime;
            mouseY = lookInput.y * gamepadSensitivity * Time.deltaTime;
        }
        else
        {
            // Mice output raw pixel movement (delta), so Time.deltaTime is not needed
            mouseX = lookInput.x * mouseSensitivity;
            mouseY = lookInput.y * mouseSensitivity;
        }

        xRotation -= mouseY;

        if (isHiding)
        {
            xRotation = Mathf.Clamp(xRotation, -30f, 30f);
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, -45f, 45f);

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            if (lightTransform) lightTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else
        {
            if (LimitVerticalLook)
                xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
            else
                xRotation = Mathf.Clamp(xRotation, -90f, 90f); 
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            if (lightTransform) lightTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX); 
    
            yRotation = 0f; 
        }
    }
}
