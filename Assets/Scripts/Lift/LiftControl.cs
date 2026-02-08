using UnityEngine;
using UnityEngine.InputSystem;

public class LiftControl : MonoBehaviour
{
    [Header("Lift Settings")]
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private bool startAtBottom = true;
    [SerializeField] private float duration = 3.0f; // Takes 3 seconds to finish

    private Transform LiftBody;
    private float elapsedTime = 0;
    private bool moving = false;
    private Vector3 startPos;
    private Vector3 endPos;
    private CharacterController playerController = null;
    private DetectObjectOutline outlineScript;
    private InputAction interactAction;

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        LiftBody = transform.parent.GetChild(0);
        startPos = LiftBody.position;
        startPos.y += minHeight;
        endPos = LiftBody.position;
        endPos.y += maxHeight;
        outlineScript = GameObject.FindGameObjectWithTag("Player").GetComponent<DetectObjectOutline>();
        outlineScript.ignoreLiftChain = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<CharacterController>();
            outlineScript.ignoreLiftChain = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = null;
            outlineScript.ignoreLiftChain = true;
        }
    }

    private void Update()
    {
        if (playerController != null && !moving && interactAction.WasPressedThisFrame())
        {
            outlineScript.ignoreLiftChain = true;
            moving = true;
            elapsedTime = 0;
        }
        if (moving)
            if (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0, 1, t);

                Vector3 newPos;
                if (startAtBottom)
                    newPos = Vector3.Lerp(startPos, endPos, t);
                else 
                    newPos = Vector3.Lerp(endPos, startPos, t);
                Vector3 platformMovement = newPos - LiftBody.position;
                LiftBody.position = newPos;
                this.transform.position = newPos; // move controller as well - just not lift chain!
                if (playerController != null) playerController.Move(platformMovement);
            }
            else
            {
                if (startAtBottom)
                {
                    LiftBody.position = endPos;
                    this.transform.position = endPos;
                }
                else
                {
                    LiftBody.position = startPos;
                    this.transform.position = startPos;
                }
                moving = false;
                startAtBottom = !startAtBottom;
                outlineScript.ignoreLiftChain = false;
            }
    }
}
