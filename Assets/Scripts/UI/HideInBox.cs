using UnityEngine;

public class HideInBox : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera boxViewCamera;    
    [SerializeField] private Transform boxAnchor;    
    [SerializeField] private float exitDistance = 1.5f; 
    
    [Header("Rotation Limits")]
    [SerializeField] private float horizontalLimit = 45f;
    [SerializeField] private float verticalLimit = 30f;
    [SerializeField] private float sensitivity = 2f;

    private bool isHiding = false;
    private GameObject playerRef;
    private Camera playerMainCamera;
    
    private float yaw;
    private float pitch;

    public void Interact(GameObject player)
    {
        if (!isHiding) EnterBox(player);
        else ExitBox();
    }

    private void EnterBox(GameObject player)
    {
        isHiding = true;
        playerRef = player;
        playerMainCamera = player.GetComponentInChildren<Camera>();

        // should disable player movements
        if(player.TryGetComponent(out CharacterController cc)) cc.enabled = false;
        
        player.transform.position = boxAnchor.position;
        player.transform.rotation = boxAnchor.rotation;

        // Switch the player cam
        playerMainCamera.gameObject.SetActive(false);
        boxViewCamera.gameObject.SetActive(true);

        yaw = 0;
        pitch = 0;
    }

    private void ExitBox()
    {
        isHiding = false;

        Vector3 exitDirection = boxAnchor.forward;

        exitDirection.y = 0; 
        exitDirection.Normalize();

        Vector3 exitPosition = boxAnchor.position + (exitDirection * exitDistance);
        playerRef.transform.position = exitPosition;
        playerRef.transform.rotation = Quaternion.LookRotation(exitDirection);

        boxViewCamera.gameObject.SetActive(false);
        playerMainCamera.gameObject.SetActive(true);
        if(playerRef.TryGetComponent(out CharacterController cc)) cc.enabled = true;

        playerRef = null;
    }

    private void Update()
    {
        if (!isHiding) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitBox();
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        // limits how much the player can rotate cam in box
        yaw = Mathf.Clamp(yaw, -horizontalLimit, horizontalLimit);
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);

        Quaternion targetRotation = boxAnchor.rotation * Quaternion.Euler(pitch, yaw, 0);
        boxViewCamera.transform.rotation = Quaternion.Slerp(boxViewCamera.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}