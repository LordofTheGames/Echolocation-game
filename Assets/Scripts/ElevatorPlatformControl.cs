using UnityEngine;

public class ElevatorPlatformControl : MonoBehaviour
{
    public Transform Lift;
    [Header("Platform Settings")]
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private bool startAtBottom = true;
    public float duration = 3.0f; // Takes 3 seconds to finish
    private float elapsedTime = 0;
    private bool moving = false;
    private Vector3 startPos;
    private Vector3 endPos;
    private CharacterController playerController = null;

    private void Start()
    {
       startPos = Lift.transform.position;
       startPos.y += minHeight;
       endPos = Lift.transform.position;
       endPos.y += maxHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = null;
        }
    }

    private void Update()
    {
        if (playerController != null && !moving && Input.GetKeyDown(KeyCode.E))
        {
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
                Vector3 platformMovement = newPos - Lift.position;
                Lift.position = newPos;
                if (playerController != null)
                {
                    playerController.Move(platformMovement);
                }
            }
            else
            {
                if (startAtBottom)
                    Lift.position = endPos;
                else
                    Lift.position = startPos;
                moving = false;
                startAtBottom = !startAtBottom;
            }
    }

    // private Vector3 lastPlatformPosition;

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.transform.CompareTag("Platform") && collision.contactCount > 0)
    //     {
    //         if (collision.GetContact(0).normal.y > 0.5f)
    //         {
    //             currentPlatform = collision.transform;
    //             lastPlatformPosition = currentPlatform.position;
    //         }
    //     }
    // }

    // private void OnCollisionStay(Collision collision)
    // {
    //     if (collision.transform.CompareTag("Platform") && currentPlatform == null && collision.contactCount > 0)
    //     {
    //         if (collision.GetContact(0).normal.y > 0.5f)
    //         {
    //             currentPlatform = collision.transform;
    //             lastPlatformPosition = currentPlatform.position;
    //         }
    //     }
    // }

    // private void OnCollisionExit(Collision collision)
    // {
    //     if (collision.transform == currentPlatform)
    //     {
    //         currentPlatform = null;
    //     }
    // }

    // private void FixedUpdate()
    // {
    //     if (currentPlatform != null)
    //     {
    //         Vector3 platformMovement = currentPlatform.position - lastPlatformPosition;

    //         if (platformMovement.y != 0)
    //         {
    //             transform.position += new Vector3(0, platformMovement.y, 0);
    //         }

    //         lastPlatformPosition = currentPlatform.position;
    //     }
    // }
}
