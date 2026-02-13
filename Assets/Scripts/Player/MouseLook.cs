using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public Transform lightTransform;
    float xRotation = 0f;
    Vector2 lookInput; // Stores the current mouse delta
    InputAction lookAction;

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {

        // If the map is disabled (Inventory open), OnLook stops updating.

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // We clamp the rotation so we cant over-rotate
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (lightTransform) lightTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);


        playerBody.Rotate(Vector3.up * mouseX); 

        // Reset input after use to prevent "drift" when mouse moving
        lookInput = Vector2.zero;
    }
}
