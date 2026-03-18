using UnityEngine;

public class CollisionEcho : MonoBehaviour
{

    [Header("Audio Settings")]
    [Tooltip("Drag impact sound here")]
    public AudioClip collisionSound;
    [Range(0f,30f)]
    public float soundVolume = 1.0f;

    [Tooltip("How far to pull back the hit point from the surface it hits so rays don't go on the wrong side - hit point is in/part of the surface")]
    public float pingOffset = 0.3f;
    public float minDelayAfterArm = 0.05f; // Prevents immediate collision detection
    private bool armed = false; // Indicates whether this object has been thrown
    // Prevents scene-placed objects from triggering echo immediately on game start
    private bool reachedMaxPings = false;
    private float armTime = -999f;

    public int currentPings = 0;
    public int maxPings = 3; // Number of bounces/collisions that will trigger echolocation
    
    [Header("Ping Settings")]
    public float angle = 360f;
    public float uniformity = 1f;
    public int numRays = 10000;
    public float visualVolume = 50f;
    public float monsterVolume = 100f;

    public void Arm() // Called by the throwing system when the object is released
    {
        armed = true;
        reachedMaxPings = false;
        armTime = Time.time;
        enabled = true;
        currentPings = 0;
    }

    // Runs automatically when the cube hits something
    private void OnCollisionEnter(Collision collision)
    {
        if (!armed || reachedMaxPings) return;

        if (Time.time - armTime < minDelayAfterArm) return; // Ignore collisions that occur too soon after throwing (prevents false triggers)

        ContactPoint contact = collision.contacts[0]; // Get contact point
        Vector3 spawnPoint = contact.point + (contact.normal * pingOffset); // Offset along normal to the surface

        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, contact.point, soundVolume);    // Plays audio at exact point of impact
        }
        else
        {
            Debug.Log("No sound attached for this type of collision");  // Warning in case a sound is meant to be attached
        }

        GlobalEchoSystem.Ping(gameObject, spawnPoint, Vector3.forward, angle, uniformity, numRays, visualVolume, monsterVolume);
        currentPings++;

        if (currentPings >= maxPings)
        {
            reachedMaxPings = true;
            enabled = false;    
        }
    }
}
