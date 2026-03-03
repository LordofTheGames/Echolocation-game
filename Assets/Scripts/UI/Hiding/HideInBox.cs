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

    private bool isHiding = false;
    private bool isTransitioning = false; // Prevents input while moving
    
    private GameObject playerRef;
    private Camera playerMainCamera;
    private Quaternion originalCameraLocalRot; // Saves the camera's neck angle
    
    private float yaw;
    private float pitch;

    public void Interact(GameObject player)
    {
        // Don't allow interaction if we are currently animating in or out
        if (isTransitioning) return; 

        if (!isHiding) StartCoroutine(EnterBoxRoutine(player));
        else StartCoroutine(ExitBoxRoutine());
    }

    private IEnumerator EnterBoxRoutine(GameObject player)
    {
        isTransitioning = true;
        playerRef = player;
        playerMainCamera = player.GetComponentInChildren<Camera>();

        // Disable player movement
        if(player.TryGetComponent(out CharacterController cc)) cc.enabled = false;

        // Save the camera's local rotation so we can restore it when exiting
        originalCameraLocalRot = playerMainCamera.transform.localRotation;

        Vector3 startPos = player.transform.position;
        Quaternion startPlayerRot = player.transform.rotation;
        Quaternion startCamRot = playerMainCamera.transform.rotation;

        float time = 0;
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = time / transitionDuration;
            
            // Smooth step equation for nicer, eased movement (starts slow, fast in middle, ends slow)
            t = t * t * (3f - 2f * t); 

            // Lerp Player Body
            player.transform.position = Vector3.Lerp(startPos, boxAnchor.position, t);
            player.transform.rotation = Quaternion.Slerp(startPlayerRot, boxAnchor.rotation, t);
            
            // Lerp Camera to look straight out of the box
            playerMainCamera.transform.rotation = Quaternion.Slerp(startCamRot, boxAnchor.rotation, t);

            yield return null; // Wait until next frame
        }

        // Snap to exact final positions just to be perfectly accurate
        player.transform.position = boxAnchor.position;
        player.transform.rotation = boxAnchor.rotation;
        playerMainCamera.transform.rotation = boxAnchor.rotation;

        yaw = 0;
        pitch = 0;

        isHiding = true;
        isTransitioning = false;
    }

    private IEnumerator ExitBoxRoutine()
    {
        isTransitioning = true;
        isHiding = false; // Immediately stop the mouse look logic in Update()

        // Calculate the safe position IN FRONT of the box
        Vector3 exitDirection = boxAnchor.forward;
        exitDirection.y = 0; 
        exitDirection.Normalize();

        Vector3 exitPos = boxAnchor.position + (exitDirection * exitDistance);
        Quaternion exitRot = Quaternion.LookRotation(exitDirection);

        Vector3 startPos = playerRef.transform.position;
        Quaternion startPlayerRot = playerRef.transform.rotation;
        Quaternion startCamRot = playerMainCamera.transform.rotation;

        // Where the camera should end up relative to the player body
        Quaternion targetCamRot = exitRot * originalCameraLocalRot;

        float time = 0;
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = time / transitionDuration;
            t = t * t * (3f - 2f * t);

            playerRef.transform.position = Vector3.Lerp(startPos, exitPos, t);
            playerRef.transform.rotation = Quaternion.Slerp(startPlayerRot, exitRot, t);
            playerMainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, t);

            yield return null;
        }

        playerRef.transform.position = exitPos;
        playerRef.transform.rotation = exitRot;
        
        // Restore the exact original local camera angle so standard FPC scripts don't break
        playerMainCamera.transform.localRotation = originalCameraLocalRot; 

        // Re-enable player movement
        if(playerRef.TryGetComponent(out CharacterController cc)) cc.enabled = true;

        playerRef = null;
        playerMainCamera = null;
        isTransitioning = false;
    }

    private void Update()
    {
        // Don't allow looking around while we are lerping in/out
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

        yaw = Mathf.Clamp(yaw, -horizontalLimit, horizontalLimit);
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);

        // Apply rotation directly to the player's main camera now
        Quaternion targetRotation = boxAnchor.rotation * Quaternion.Euler(pitch, yaw, 0);
        playerMainCamera.transform.rotation = Quaternion.Slerp(playerMainCamera.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}