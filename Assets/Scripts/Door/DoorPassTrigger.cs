using UnityEngine;

public class DoorPassTrigger : MonoBehaviour
{
    [SerializeField] private LabDoor labDoor;

    private Transform playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (labDoor == null) return;

        playerInside = other.transform;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (labDoor == null) return;

        labDoor.TryMarkPassed(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInside == other.transform)
            playerInside = null;
    }
}