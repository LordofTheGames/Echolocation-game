using UnityEngine;

public class CollisionEcho : MonoBehaviour
{
    [Tooltip("How far to pull back the hit point from the surface it hits so rays don't go on the wrong side - hit point is in/part of the surface")]
    public float pingOffset = 0.3f;
    public float minDelayAfterArm = 0.05f; // Prevents immediate collision detection
    private bool armed = false; // Indicates whether this object has been thrown
    // Prevents scene-placed objects from triggering echo immediately on game start
    private bool reachedMaxPings = false;
    private float armTime = -999f;

    public int currentPings = 0;
    public int maxPings = 3; // Number of bounces/collisions that will trigger echolocation

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

        GlobalEchoSystem.Ping(gameObject, spawnPoint); // Ping the echolocation system with the point of contact and object that spawned it
        currentPings++;

        if (currentPings >= maxPings)
        {
            reachedMaxPings = true;
            enabled = false;    
        }
    }
}
