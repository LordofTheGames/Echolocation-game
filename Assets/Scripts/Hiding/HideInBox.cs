using System.Collections;
using UnityEngine;
using System;
using Unity.Behavior;

public class HideInBox : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform boxAnchor;    
    [SerializeField] private float exitDistance = 1.5f; 
    [SerializeField] private float transitionDuration = 1f; // How long the lerp takes
    
    private float originalPlayerY;

    private bool isHiding = false;
    private bool isTransitioning = false; // Prevents bugs if player spams interact
    private float hidingTime;
    
    private BehaviorGraphAgent agent;
    private GameObject navmeshEdges;
    private GameObject player;
    private GameObject playerRef;
    private Camera playerMainCamera;
    private Quaternion originalCamLocalRot; // Saves original neck angle

    public static event System.Action OnPlayerHide;
    public static event System.Action OnPlayerExit;

    
    void Start()
    {
        agent = GameObject.Find("Monster").GetComponent<BehaviorGraphAgent>();
        player = GameObject.FindGameObjectWithTag("Player"); 
        navmeshEdges = GameObject.Find("NavMesh Edges");
    }

    public void Interact()
    {
        if (isTransitioning) return;

        if (!isHiding) StartCoroutine(EnterBoxRoutine(player));
        else StartCoroutine(ExitBoxRoutine());
    }

    private IEnumerator EnterBoxRoutine(GameObject player)
    {
        isTransitioning = true;
        playerRef = player;
        playerMainCamera = player.GetComponentInChildren<Camera>();

        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponentInChildren<MouseLook>().hidingTransition = true; 

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
            float ease = clampedT * clampedT * (3f - 2f * clampedT);

            // Lerp position and rotations smoothly
            player.transform.position = Vector3.Lerp(startPos, boxAnchor.position, ease);
            player.transform.rotation = Quaternion.Slerp(startPlayerRot, boxAnchor.rotation, ease);
            playerMainCamera.transform.rotation = Quaternion.Slerp(startCamRot, boxAnchor.rotation, ease);

            yield return null; // Wait for next frame
        }

        hidingTime = 0;
        agent.BlackboardReference.SetVariableValue("hidingTime", 0f);
        isHiding = true;
        OnPlayerHide?.Invoke(); // Tell the game the player hid
        agent.BlackboardReference.SetVariableValue("playerIsHiding", true);
        navmeshEdges.SetActive(false);
        isTransitioning = false;

        MouseLook ml = player.GetComponentInChildren<MouseLook>();
        ml.SyncLookAngles(0f, 0f);
        ml.isHiding = true; 
        ml.hidingTransition = false;
    }

    private IEnumerator ExitBoxRoutine()
    {
        isTransitioning = true;
        isHiding = false; 
        OnPlayerExit?.Invoke();
        player.GetComponentInChildren<MouseLook>().hidingTransition = true; 
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

        playerRef.transform.position = exitPosition;
        playerRef.transform.rotation = exitPlayerRot;

        playerMainCamera.transform.localRotation = originalCamLocalRot; 
        Physics.SyncTransforms();

        float originalPitch = originalCamLocalRot.eulerAngles.x;
        if (originalPitch > 180f) originalPitch -= 360f;
        MouseLook ml = playerRef.GetComponentInChildren<MouseLook>();
        ml.SyncLookAngles(originalPitch, 0f);

        // Re-enable player movements
        playerRef.GetComponent<CharacterController>().enabled = true;
        playerRef.GetComponent<PlayerMovement>().enabled = true;
        ml.isHiding = false; 
        ml.hidingTransition = false; 
        agent.BlackboardReference.SetVariableValue("playerIsHiding", false);
        agent.BlackboardReference.SetVariableValue("exitHiding", false);
        navmeshEdges.SetActive(true);

        playerRef = null;
        isTransitioning = false;
    }

    // used for registering time player has been hiding for
    void Update()
    {
       if (isHiding)
        {
            hidingTime += Time.deltaTime;
            agent.BlackboardReference.SetVariableValue("hidingTime", hidingTime);
        } 
    }
}