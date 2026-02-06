using UnityEngine;

public class LiftControl : MonoBehaviour
{
    public Transform Lift;
    [Header("Lift Settings")]
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
}
