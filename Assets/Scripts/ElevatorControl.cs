using UnityEngine;

public class ElevatorControl : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private bool startAtBottom = true;

    [Header("Component References")]
    [SerializeField] private Transform platformTransform;
    [SerializeField] private GameObject interactionUI;

    private bool playerInTrigger = false;
    private bool isMoving = false;
    private bool isMovingUp = false;
    private Vector3 targetPosition;

    private void Start()
    {
        if (platformTransform == null)
        {
            platformTransform = this.transform;
        }

        if (startAtBottom)
        {
            platformTransform.position = new Vector3(
                platformTransform.position.x,
                minHeight,
                platformTransform.position.z
            );
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            TogglePlatformMovement();
        }

        if (isMoving)
        {
            MovePlatform();
        }
    }

    private void TogglePlatformMovement()
    {
        if (isMoving)
        {
            isMoving = false;
        }
        else
        {
            float currentHeight = platformTransform.position.y;

            if (Mathf.Abs(currentHeight - minHeight) < 0.1f)
            {
                isMovingUp = true;
                targetPosition = new Vector3(
                    platformTransform.position.x,
                    maxHeight,
                    platformTransform.position.z
                );
            }
            else if (Mathf.Abs(currentHeight - maxHeight) < 0.1f)
            {
                isMovingUp = false;
                targetPosition = new Vector3(
                    platformTransform.position.x,
                    minHeight,
                    platformTransform.position.z
                );
            }
            else
            {
                isMovingUp = !isMovingUp;
                targetPosition = isMovingUp ?
                    new Vector3(platformTransform.position.x, maxHeight, platformTransform.position.z) :
                    new Vector3(platformTransform.position.x, minHeight, platformTransform.position.z);
            }

            isMoving = true;
        }
    }

    private void MovePlatform()
    {
        float step = moveSpeed * Time.deltaTime;
        platformTransform.position = Vector3.MoveTowards(
            platformTransform.position,
            targetPosition,
            step
        );

        if (Vector3.Distance(platformTransform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            platformTransform.position = targetPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;

            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}
