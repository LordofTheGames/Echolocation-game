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
}
