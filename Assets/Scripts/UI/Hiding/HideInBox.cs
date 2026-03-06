using System.Collections;
using UnityEngine;

public class HideInBox : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform boxAnchor;    
    [SerializeField] private float exitDistance = 1.5f; 
    [SerializeField] private float transitionDuration = 0.5f; // How long the lerp takes
    
    [Header("Rotation Limits")]
    [SerializeField] private float horizontalLimit = 45f;
    [SerializeField] private float verticalLimit = 30f;
    [SerializeField] private float sensitivity = 2f;

    private float originalPlayerY;

    private bool isHiding = false;
    private bool isTransitioning = false; // Prevents bugs if player spams interact
    
    private GameObject playerRef;
    private Camera playerMainCamera;
    private Quaternion originalCamLocalRot; // Saves original neck angle
    
    private float yaw;
    private float pitch;

    public void Interact(GameObject player)
    {
        if (isTransitioning) return; // Do nothing if we are currently lerping

        if (!isHiding) StartCoroutine(EnterBoxRoutine(player));
        else StartCoroutine(ExitBoxRoutine());
    }

    private IEnumerator EnterBoxRoutine(GameObject player)
    {
        isTransitioning = true;
        playerRef = player;
        playerMainCamera = player.GetComponentInChildren<Camera>();

        // Disable player movements safely
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponentInChildren<MouseLook>().enabled = false;

        // Save the FPC camera's local rotation to restore upon exit
        originalCamLocalRot = playerMainCamera.transform.localRotation;

        originalPlayerY = player.transform.position.y;

        Vector3 startPos = player.transform.position;
        Quaternion startPlayerRot = player.transform.rotation;
        Quaternion startCamRot = playerMainCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            // Add time, but use Clamp01 so the final frame calculates exactly 100% (1.0)
            t += Time.deltaTime / transitionDuration;
            float clampedT = Mathf.Clamp01(t); 

            // Smooth easing
            float ease = clampedT * clampedT * (3f - 2f * clampedT);

            // Lerp position and rotations smoothly
            player.transform.position = Vector3.Lerp(startPos, boxAnchor.position, ease);
            player.transform.rotation = Quaternion.Slerp(startPlayerRot, boxAnchor.rotation, ease);
            playerMainCamera.transform.rotation = Quaternion.Slerp(startCamRot, boxAnchor.rotation, ease);

            yield return null; // Wait for next frame
        }

        yaw = 0;
        pitch = 0;
        isHiding = true;
        isTransitioning = false;
    }

    private IEnumerator ExitBoxRoutine()
    {
        isTransitioning = true;
        isHiding = false; 

        Vector3 exitDirection = boxAnchor.forward;
        exitDirection.y = 0; 
        exitDirection.Normalize();

        Vector3 exitPosition = boxAnchor.position + (exitDirection * exitDistance);
        exitPosition.y = originalPlayerY;
        Quaternion exitPlayerRot = Quaternion.LookRotation(exitDirection);
        
        Vector3 startPos = playerRef.transform.position;
        Quaternion startPlayerRot = playerRef.transform.rotation;
        Quaternion startCamRot = playerMainCamera.transform.rotation;
        
        Quaternion targetCamRot = exitPlayerRot * originalCamLocalRot;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float clampedT = Mathf.Clamp01(t);
            float ease = Mathf.SmoothStep(0f, 1f, clampedT);

            playerRef.transform.position = Vector3.Lerp(startPos, exitPosition, ease);
            playerRef.transform.rotation = Quaternion.Slerp(startPlayerRot, exitPlayerRot, ease);
            playerMainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, ease);

            yield return null;
        }

        // FIX 1: Explicitly lock in the final 100% transforms
        playerRef.transform.position = exitPosition;
        playerRef.transform.rotation = exitPlayerRot;
        playerMainCamera.transform.rotation = targetCamRot;

        // Apply local rotation now that the parent transform is 100% perfectly aligned
        playerMainCamera.transform.localRotation = originalCamLocalRot;

        // FIX 2: Force Unity to update physics transforms BEFORE turning the CharacterController back on
        Physics.SyncTransforms();

        // Re-enable player movements
        playerRef.GetComponent<CharacterController>().enabled = true;
        playerRef.GetComponent<PlayerMovement>().enabled = true;
        playerRef.GetComponentInChildren<MouseLook>().enabled = true;

        playerRef = null;
        isTransitioning = false;
    }

    private void Update()
    {
        // Don't allow camera movement while transitioning
        if (!isHiding || isTransitioning) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ExitBoxRoutine());
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        // limits how much the player can rotate cam in box
        yaw = Mathf.Clamp(yaw, -horizontalLimit, horizontalLimit);
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);

        // Apply clamped rotation relative to the box anchor's forward direction
        Quaternion targetRotation = boxAnchor.rotation * Quaternion.Euler(pitch, yaw, 0);
        playerMainCamera.transform.rotation = targetRotation;
    }
}