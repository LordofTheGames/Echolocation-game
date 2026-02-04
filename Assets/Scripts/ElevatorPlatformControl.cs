using UnityEngine;

public class ElevatorPlatformControl : MonoBehaviour
{
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Platform") && collision.contactCount > 0)
        {
            if (collision.GetContact(0).normal.y > 0.5f)
            {
                currentPlatform = collision.transform;
                lastPlatformPosition = currentPlatform.position;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("Platform") && currentPlatform == null && collision.contactCount > 0)
        {
            if (collision.GetContact(0).normal.y > 0.5f)
            {
                currentPlatform = collision.transform;
                lastPlatformPosition = currentPlatform.position;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform == currentPlatform)
        {
            currentPlatform = null;
        }
    }

    private void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            Vector3 platformMovement = currentPlatform.position - lastPlatformPosition;

            if (platformMovement.y != 0)
            {
                transform.position += new Vector3(0, platformMovement.y, 0);
            }

            lastPlatformPosition = currentPlatform.position;
        }
    }
}
