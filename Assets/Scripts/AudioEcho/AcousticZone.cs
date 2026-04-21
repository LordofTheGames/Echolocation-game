using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AcousticZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("The ideal radius of the listener sphere when the player is inside this specific room.")]
    public float roomRadius = 20f;

    void Awake()
    {
        // Ensure the collider is set to Trigger so the player can walk through it
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider enteringObject)
    {
        // If the player walks into this invisible box, tell the receiver to start resizing!
        if (enteringObject.CompareTag("Player") && AcousticReceiver.Instance != null)
        {
            AcousticReceiver.Instance.SetTargetRadius(roomRadius);
        }
    }
}