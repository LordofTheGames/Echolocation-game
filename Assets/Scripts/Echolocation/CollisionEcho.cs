using UnityEngine;

public class TempCubeEcholocate : MonoBehaviour
{

    [Tooltip("How far to pull back the hit point from the surface it hits so rays don't go on the wrong side - hit point is in/part of the surface")]
    public float pingOffset = 0.3f;
    bool hasPinged = false;

    // Runs automatically when the cube hits something
    void OnCollisionEnter(Collision collision)
    {
        // Ensure only one ping per cube - first hit
        if (!hasPinged)
        {
            hasPinged = true;

            ContactPoint contact = collision.contacts[0]; // Get contact point

            Vector3 spawnPoint = contact.point + (contact.normal * pingOffset); // Offset along normal to the surface

            GlobalEchoSystem.Ping(spawnPoint, gameObject); // Ping the echolocation system with the point of contact and object that spawned it

            Destroy(this); // Destroy this script so it doesn't keep calculating physics logic
        }
    }
}
