using UnityEngine;
using UnityEngine.InputSystem;

public class TempEcholocationTrigger : MonoBehaviour
{
    [Header("Pulse Origins")]
    public Transform cameraTransform; // Assign main camera here

    [Header("Echo Projection Uniformity (0 = clumped, 1 = uniform)")]
    public float uniformity = 1.0f;

    [Header("Number of Rays")]
    public int numRays = 4000;

    [Header("Angle of projection (0 = line, 60 = cone, 360 = sphere)")]
    public float angle = 60f;

    public void OnEcholocate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, angle, uniformity, numRays);
        }
    }
}
